import cv2
import math
import time
import mediapipe as mp
from state import SharedState
from api_server import send_command_to_unity
from collections import deque

mp_face_mesh = mp.solutions.face_mesh

CAMERA_INDEX = 0
FRAME_WIDTH = 640
FRAME_HEIGHT = 480

YAW_THRESHOLD = 0.065
PITCH_UP_THRESHOLD = 0.30
PITCH_DOWN_THRESHOLD = 0.50

SMOOTHING_ALPHA = 0.30

NOSE_ID = 1
LEFT_EYE_ID = 33
RIGHT_EYE_ID = 263
CHIN_ID = 152

last_sent_command = "NEUTRAL"

COMMAND_HISTORY_SIZE = 5
command_history = deque(maxlen=COMMAND_HISTORY_SIZE)

smooth_yaw = None
smooth_pitch_ratio = None


def set_neutral():
    global last_sent_command

    if SharedState.current_command != "NEUTRAL":
        SharedState.current_command = "NEUTRAL"
        print("Command -> NEUTRAL")

    last_sent_command = "NEUTRAL"


def send_command(command):
    global last_sent_command

    SharedState.current_command = command
    SharedState.command_id += 1
    last_sent_command = command

    send_command_to_unity(SharedState.current_command, SharedState.command_id)

    print(f"Command -> {command} | id={SharedState.command_id}")


def distance_2d(p1, p2):
    return math.sqrt((p1[0] - p2[0]) ** 2 + (p1[1] - p2[1]) ** 2)


def midpoint(p1, p2):
    return ((p1[0] + p2[0]) / 2.0, (p1[1] + p2[1]) / 2.0)


def clamp_min(value, minimum=1e-6):
    return value if value > minimum else minimum


def smooth_value(current_value, previous_value):
    if previous_value is None:
        return current_value
    return (SMOOTHING_ALPHA * current_value) + ((1.0 - SMOOTHING_ALPHA) * previous_value)


def get_face_points(face_landmarks):
    nose = face_landmarks.landmark[NOSE_ID]
    left_eye = face_landmarks.landmark[LEFT_EYE_ID]
    right_eye = face_landmarks.landmark[RIGHT_EYE_ID]
    chin = face_landmarks.landmark[CHIN_ID]

    nose_p = (nose.x, nose.y)
    left_eye_p = (left_eye.x, left_eye.y)
    right_eye_p = (right_eye.x, right_eye.y)
    chin_p = (chin.x, chin.y)

    return nose_p, left_eye_p, right_eye_p, chin_p


def calculate_yaw_ratio(nose_p, left_eye_p, right_eye_p):
    d_left = distance_2d(nose_p, left_eye_p)
    d_right = distance_2d(nose_p, right_eye_p)

    denom = clamp_min(d_left + d_right)
    yaw_ratio = (d_left - d_right) / denom
    return yaw_ratio


def calculate_pitch_ratio(nose_p, left_eye_p, right_eye_p, chin_p):
    eyes_center = midpoint(left_eye_p, right_eye_p)

    vertical_total = chin_p[1] - eyes_center[1]
    vertical_total = clamp_min(abs(vertical_total))

    vertical_nose = nose_p[1] - eyes_center[1]

    pitch_ratio = vertical_nose / vertical_total
    return pitch_ratio


def get_command(yaw_ratio, pitch_ratio):
    if yaw_ratio > YAW_THRESHOLD:
        return "RIGHT"

    if yaw_ratio < -YAW_THRESHOLD:
        return "LEFT"

    if pitch_ratio < PITCH_UP_THRESHOLD:
        return "JUMP"

    if pitch_ratio > PITCH_DOWN_THRESHOLD:
        return "SLIDE"

    return "NEUTRAL"


def run_detector():
    global smooth_yaw, smooth_pitch_ratio, last_sent_command

    cap = cv2.VideoCapture(CAMERA_INDEX)
    cap.set(cv2.CAP_PROP_FRAME_WIDTH, FRAME_WIDTH)
    cap.set(cv2.CAP_PROP_FRAME_HEIGHT, FRAME_HEIGHT)

    if not cap.isOpened():
        raise RuntimeError("Camera could not be opened.")

    with mp_face_mesh.FaceMesh(
        static_image_mode=False,
        max_num_faces=1,
        refine_landmarks=False,
        min_detection_confidence=0.7,
        min_tracking_confidence=0.7
    ) as face_mesh:

        paused = False

        while cap.isOpened():
            success, frame = cap.read()
            if not success:
                print("Could not read frame from camera.")
                break

            frame = cv2.flip(frame, 1)
            h, w, _ = frame.shape

            display_command = SharedState.current_command
            status_text = ""

            if not paused:
                rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
                results = face_mesh.process(rgb)

                if results.multi_face_landmarks:
                    face_landmarks = results.multi_face_landmarks[0]

                    nose_p, left_eye_p, right_eye_p, chin_p = get_face_points(face_landmarks)

                    nose_px = (int(nose_p[0] * w), int(nose_p[1] * h))
                    left_eye_px = (int(left_eye_p[0] * w), int(left_eye_p[1] * h))
                    right_eye_px = (int(right_eye_p[0] * w), int(right_eye_p[1] * h))
                    chin_px = (int(chin_p[0] * w), int(chin_p[1] * h))

                    eyes_center = midpoint(left_eye_p, right_eye_p)
                    eyes_center_px = (int(eyes_center[0] * w), int(eyes_center[1] * h))

                    cv2.circle(frame, nose_px, 5, (0, 255, 0), -1)
                    cv2.circle(frame, left_eye_px, 5, (255, 0, 0), -1)
                    cv2.circle(frame, right_eye_px, 5, (0, 0, 255), -1)
                    cv2.circle(frame, chin_px, 5, (255, 255, 0), -1)
                    cv2.circle(frame, eyes_center_px, 5, (255, 0, 255), -1)

                    cv2.line(frame, left_eye_px, right_eye_px, (255, 255, 255), 2)
                    cv2.line(frame, eyes_center_px, chin_px, (0, 255, 255), 2)

                    yaw_ratio = calculate_yaw_ratio(nose_p, left_eye_p, right_eye_p)
                    pitch_ratio = calculate_pitch_ratio(nose_p, left_eye_p, right_eye_p, chin_p)

                    smooth_yaw = smooth_value(yaw_ratio, smooth_yaw)
                    smooth_pitch_ratio = smooth_value(pitch_ratio, smooth_pitch_ratio)

                    detected_command = get_command(smooth_yaw, smooth_pitch_ratio)
                    command_history.append(detected_command)

                    final_command = max(set(command_history), key=command_history.count)

                    if final_command == "NEUTRAL":
                        set_neutral()
                    else:
                        if final_command != last_sent_command:
                            send_command(final_command)

                    status_text = "Tracking"

                    cv2.putText(frame, f"yaw={smooth_yaw:.3f}", (20, 40),
                                cv2.FONT_HERSHEY_SIMPLEX, 0.7, (0, 255, 0), 2)

                    cv2.putText(frame, f"pitch_ratio={smooth_pitch_ratio:.3f}", (20, 75),
                                cv2.FONT_HERSHEY_SIMPLEX, 0.7, (0, 255, 255), 2)
                else:
                    set_neutral()
                    display_command = "NO FACE"
                    status_text = "No face detected"
            else:
                set_neutral()
                display_command = "PAUSED"
                status_text = "Paused"

            if display_command not in ["NO FACE", "PAUSED"]:
                display_command = SharedState.current_command

            cv2.putText(frame, f"Command: {display_command}", (20, 115),
                        cv2.FONT_HERSHEY_SIMPLEX, 0.9, (0, 0, 255), 2)

            cv2.putText(frame, f"Command ID: {SharedState.command_id}", (20, 150),
                        cv2.FONT_HERSHEY_SIMPLEX, 0.7, (255, 255, 255), 2)

            cv2.putText(frame, f"Status: {status_text}", (20, 185),
                        cv2.FONT_HERSHEY_SIMPLEX, 0.7, (255, 255, 0), 2)

            cv2.putText(frame, "Q or ESC: Exit | P: Pause", (20, h - 20),
                        cv2.FONT_HERSHEY_SIMPLEX, 0.6, (255, 255, 255), 2)

            time.sleep(0.01)

    cap.release()
    cv2.destroyAllWindows()
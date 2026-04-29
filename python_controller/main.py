import threading
from api_server import app, run_websocket_server
from HeadTracking.detector import run_detector
from state import SharedState

def run_api():
    print("Flask API started on http://127.0.0.1:5000")
    app.run(host="127.0.0.1", port=5000, debug=False, use_reloader=False)

if __name__ == "__main__":
    SharedState.current_command = "NEUTRAL"
    SharedState.command_id = 0

    websocket_thread = threading.Thread(target=run_websocket_server, daemon=True)
    websocket_thread.start()

    detector_thread = threading.Thread(target=run_detector, daemon=True)
    detector_thread.start()

    run_api()

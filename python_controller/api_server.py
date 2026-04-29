import asyncio
import json
from flask import Flask, jsonify
import websockets

app = Flask(__name__)

connected_clients = set()
ws_loop = None

@app.route("/health", methods=["GET"])
def health():
    return jsonify({"status": "ok"}), 200

async def websocket_handler(websocket):
    connected_clients.add(websocket)
    print("WebSocket client connected.")

    try:
        await websocket.wait_closed()
    finally:
        connected_clients.discard(websocket)
        print("WebSocket client disconnected.")

async def start_websocket_server():
    server = await websockets.serve(websocket_handler, "127.0.0.1", 8765)
    print("WebSocket server started on ws://127.0.0.1:8765")
    await server.wait_closed()

def run_websocket_server():
    global ws_loop
    ws_loop = asyncio.new_event_loop()
    asyncio.set_event_loop(ws_loop)
    ws_loop.run_until_complete(start_websocket_server())

async def broadcast_command(command, command_id):
    if not connected_clients:
        return

    message = json.dumps({
        "command": command,
        "command_id": command_id
    })

    dead = set()

    for client in connected_clients:
        try:
            await client.send(message)
        except Exception:
            dead.add(client)

    for client in dead:
        connected_clients.discard(client)

def send_command_to_unity(command, command_id):
    if ws_loop is None:
        return

    asyncio.run_coroutine_threadsafe(
        broadcast_command(command, command_id),
        ws_loop
    )
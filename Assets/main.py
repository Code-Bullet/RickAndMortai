import zmq
import pytchat
import time

# Initialize ZeroMQ context
context = zmq.Context()

# Create a REP (Reply) socket
socket = context.socket(zmq.REP)

# Bind the socket to a TCP address
socket.bind("tcp://127.0.0.1:5555")

# Initialize YouTube chat; the video id is the code in the URL after "v="
chat = pytchat.create(video_id="28SjdiSV95w")

while True:
   if not chat.is_alive():
       print("Chat is not alive. Reinitializing...")
       chat = pytchat.create(video_id="28SjdiSV95w")

   chat_messages = []

   for c in chat.get().sync_items():
       message = f"{c.message}\n{c.author.name}\n"
       chat_messages.append(message)

   # Wait for a request from Unity
   unity_request = socket.recv_string()

   if unity_request == "get_chat":
       # Send chat messages to Unity as a single string
       socket.send_string("\n".join(chat_messages))


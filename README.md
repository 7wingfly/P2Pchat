
UDP Hole-Punching
================

Purpose
-----------

Have a server negotiate a peer-to-peer connection between two clients in the most direct way possible. 

Demonstrate Hole-Punching / NAT traversal using the UDP protocol.

Demonstrate the support of UPnP.

Method
----------

Server:

* Uses TCP to manage available clients.
* Uses UDP to learn the public End Point (IP and Port) of clients.
* Publishes list of connected clients and their End Points.
* Relays connection requests between clients.

Client:

* Attempts to use UPnP to forward a port to itself on the router (Optional).
* Sends the server the following information:
  - Hostname (as a friendly name)
  - Whether UPnP is enabled or not.
  - Local UDP port that the client is listening on.
  - List of local IP addresses.
* Sends a message to the server that it wishes to connect to another client / Receives a message from the server that another client wishes to connect to it. (Both clients will attempt to connect to each other at the same time)
* Sends 3 ACK requests to each local IP of other client to determine if clients are on the same LAN.
* If there was no response, sends 99 ACK requests to public/external End Point.
* If UDP Hole-Punching is successful, a chat window will be shown and a conversation can begin.

Everything the client attempts is outputted to a textbox for review.

Instructions
------------

1. Run the GetConnected server executable on a publicly reachable server. (Forward TCP and UDP port to your server on your router.)

2. Change value 'ServerEndpoint' in Client.cs to your servers public IP.

2. Run the P2PClient executable on PCs you want to connect. 

3. Click the 'Connect' button in the top left to connect to the server. The server will ensure you always have an updated list of available clients.

4. Select a client and click 'Connect' in the bottom right to connect to it.

5. Observe as two clients begin to establish a connect to each other.

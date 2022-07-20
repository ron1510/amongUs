from audioop import add
from doctest import FAIL_FAST
from itertools import count
from math import fabs
from operator import le
from pydoc import cli
import Server
import json
import random

start = False

startTime = 0
server = Server.Server()
players = []
while True:
    sendToAll = True
    data = server.Receive()
    if data == None:
        continue
    data, address = data
    data = json.loads(data.decode())
    if "playerData" in data:
        print(data["playerData"]["id"])
        print(data["playerData"]["transform"]["position"])
        print(address)

    if data["Event"] == "Connection":
        sendToAll = False
        if len(server.clients) == 1:
            message = {"type":"host","id":len(server.clients)-1}
            players.append(len(server.clients)-1)
            server.Send(json.dumps(message).encode(), address)
            continue
        message = {"type":"member","id":len(server.clients)-1}
        server.Send(json.dumps(message).encode(), address)
        players.append(len(server.clients)-1)
    if (data["Event"] == "start" or len(players) == 4) and not start:
        start = True
        sendToAll = False

        impostorAddress = random.choice(server.clients)
        messageImposter = {"type":"impostor ID","id" : server.clients.index(impostorAddress)}

        for i in server.clients:
            message = {"type":"playerList","numberOfPlayers":(len(server.clients)),"playerList":players}
            server.Send(json.dumps(messageImposter).encode(),i)
            server.Send(json.dumps(message).encode(), i)

        message1 = {"type":"role", "isImpostor": True}
        message2 = {"type":"role", "isImpostor": False}

        for i in server.clients:
            if i == impostorAddress:
                server.Send(json.dumps(message1).encode(), i)
            else:
                server.Send(json.dumps(message2).encode(), i)

            
    if sendToAll:
        for i in server.clients:
            if i != address or data["type"] == "voting" or data["type"] == "finishTask":
                server.Send(json.dumps(data).encode(),i)
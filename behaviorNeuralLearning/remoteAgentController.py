# context: IAS
# creation: 24-aug-2018 pierre.chevaillier@enib.fr adapted from existing code

import sys, getopt, socket
from os import path
import struct

# Specific modules
import numpy as np

# Home made stuffs
from agentModelAdapter import AgentModelAdapter
from neuralController import NeuralVelocityController

# ==============================================================================

def parseCommandLine():
    try:
        opts, args = getopt.getopt(sys.argv[1:], "hm:v", ["help", "model="])
    except getopt.GetoptError as err:
        print(str(err))
        sys.exit(2)
    verbose = False
    for opt, arg in opts:
        if opt == "-v":
            verbose = True
        elif opt in ("-h", "--help"):
            getopt.usage()
            sys.exit()

    return opts, args

def launchServer():
    LOCAL_IP = "127.0.0.1"
    SERVER_PORT = 5005
    read_sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    send_sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    read_sock.bind((LOCAL_IP, SERVER_PORT))
    return read_sock, send_sock

def parseData(rawData):
    dataFormat = '<ffffffffffff' # little-indian - 4x3 floats
    values = struct.unpack(dataFormat, rawData)
    # print(values)
    data = np.array(values)
    return data

def processData(dataAdapter, BehaviorController, data):
    inputData = dataAdapter.prepareInputData(data)
    result = BehaviorController.process(inputData)
    # print(str(inputData) + ' --> ' + str(result))
    return result

def formatAnswer(result):
    dataFormat = '<ff' # little-indian - 2 floats
    rawAnswer = struct.pack(dataFormat, result[0], result[1])
    return rawAnswer

# ==============================================================================
def main():
    opts, args = parseCommandLine()
    
    dataAdapter = AgentModelAdapter()
    behaviorController = NeuralVelocityController()
    behaviorController.configure(opts, args)
    behaviorController.build()

    read_sock, send_sock = launchServer()
    CLIENT_IP = "127.0.0.1"
    CLIENT_PORT = 5006

    while True:
        print("Waiting for data...")
        rawData, addr = read_sock.recvfrom(2048)
        data = parseData(rawData)
        result = processData(dataAdapter, behaviorController, data)
        rawAnswer = formatAnswer(result)
        send_sock.sendto(rawAnswer, (CLIENT_IP, CLIENT_PORT))

# ==============================================================================
if __name__ == "__main__":
    main()

# end of file
# ==============================================================================

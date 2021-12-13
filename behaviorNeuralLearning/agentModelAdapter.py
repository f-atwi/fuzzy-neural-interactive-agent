# ==============================================================================
# context: teaching - Shared Educational Resources in Computer Science
#          ENIB: module IAS - lab work on neural networks (since fall'18)
#                           - student project (fall'21)
# description: plays the role of an adapter.
#     transform the raw data to those expected as inputs of the controller
# copyright (c) 2018-2021 ENIB. All rights reserved.
# ------------------------------------------------------------------------------
# usage: python 3.x
# creation: 29-jun-2018
# revision: 05-nov-2021 pierre.chevaillier@enib.fr comments
# ------------------------------------------------------------------------------
# comments:
# warnings:
# - for educational purposes only
# todos:
# - TODO adapt to the problem (variables)
# ==============================================================================
# Python standard distribution
# None

# Specific modules
import numpy
import re
from pathlib import Path
from time import time
# Home made stuffs
# None

# ==============================================================================
# Locally defined fucntions (used in other modules)

DEFAULT_STATE = 1

def pointToPointDistance(p1, p2):
    return numpy.sqrt((p2[0] - p1[0])**2 + (p2[1] - p1[1])**2 + (p2[2] - p1[2])**2)

def clampToPiMinusPi(angle):
    if angle > numpy.pi:
        angle -= 2 * numpy.pi
    elif angle < - numpy.pi:
        angle+= 2 * numpy.pi
    return angle

# ==============================================================================
class AgentModelAdapter:
    def __init__(self, dataFilePath = str(Path("data/coolAngryModel.csv")) ):
        self.agentPosition = numpy.zeros(3)
        self.agentOrientation = numpy.zeros(3)
        self.targetPosition = numpy.zeros(3)
        self.targetOrientation = numpy.zeros(3)
        self.agentLinearVelocity = 0.0
        self.agentAngularVelocity = 0.0

        print("Load data from " + dataFilePath)
        sourceFile = open(dataFilePath, "r")

        line = sourceFile.readline()
        tokens = re.split(";", line)
        distances = []
        for t in tokens:
            distances.append(float(t))
        self.dist_step = distances[1]-distances[0
        ]
        line = sourceFile.readline()
        tokens = re.split(";", line)
        azimuths = []
        for t in tokens:
            azimuths.append(float(t))
        self.az_step = azimuths[1] - azimuths[0]
        

        line = sourceFile.readline()
        tokens = re.split(";", line)
        velocities = []
        for t in tokens:
            velocities.append(float(t))
        self.vel_step = velocities[1] - velocities[0]

        rest = sourceFile.read()
        results = re.split("\n", rest)
        results.pop(-1)

        # results = [float(x)*100 for x in results]
        self.records = numpy.empty((0, 4))
        index = 0
        for distance in distances:
            for azimuth in azimuths:
                for velocity in velocities:
                    if float(results[index])<9:
                        self.records = numpy.append(self.records, [[float(distance), float(azimuth), float(velocity), float(results[index])]], axis=0)
                    index +=1

    def initData(self, data):
        # not efficient, but easier to handle
        self.agentPosition = numpy.array(data[0:3], float)
        self.agentOrientation = numpy.array(data[3:6], float)
        self.targetPosition = numpy.array(data[6:9], float)
        self.targetOrientation = numpy.array(data[9:12], float)  

    def prepareInputData(self, data):
        self.initData(data)
        feature = [0,0,0]
        feature[0], feature[1] = self.targetRelativeLocation()
        feature[2] = self.mood(feature[0])
        return numpy.array([feature])

    def targetLinearVelocity(self):
        current_time = time()
        try:
            deltaT = current_time - self.prev_time
            tLinearVelocity = pointToPointDistance(self.targetPosition, self.prev_t_position) 
            tLinearVelocity /= deltaT

            self.prev_time = current_time
            self.prev_t_position = self.targetPosition
        except:
            self.prev_time=current_time
            self.prev_t_position=self.targetPosition
            return 0
        return tLinearVelocity

    def agentDirectionOfMove(self):
        return clampToPiMinusPi(numpy.pi / 2 - self.agentOrientation[1])

    def targetDirectionOfMove(self):
        return clampToPiMinusPi(numpy.pi / 2 - self.targetOrientation[1])

    def mood(self, dist):
        # return DEFAULT_STATE
        vel = self.targetLinearVelocity()
        # print(dist, theta, vel)
        # print(self.dist_step, self.az_step, self.vel_step)

        
        aPosT = self.agentPosition - self.targetPosition
        tzim = numpy.arctan2(aPosT[2], aPosT[0])
        tPsi = self.targetDirectionOfMove()
        theta = clampToPiMinusPi(tPsi - tzim)
     

        for record in self.records:
            if (abs(dist - record[0])<self.dist_step/2  or dist>20 or dist<0)  and (abs(theta - record[1])<self.az_step/2) and (abs(vel - record[2])<self.vel_step/2 or vel > 2 ):
                # print(record)
                if record[3] > 0.5:
                    return 1.0
                else:
                    return 0.0
        
        print(dist, theta, vel)
        return DEFAULT_STATE 

    def targetRelativeLocation(self):
        dist = pointToPointDistance(self.targetPosition, self.agentPosition)
        if dist > 0:
            tPosA = self.targetPosition - self.agentPosition
            azim = numpy.arctan2(tPosA[2], tPosA[0])
            aPsi = self.agentDirectionOfMove()
            theta = clampToPiMinusPi(aPsi - azim)
        else:
            theta = 0.0
        return dist, theta

# ------------------------------------------------------------------------------
def unitaryTests():
    adapter = AgentModelAdapter()

    testData = numpy.array([-1, -2, -3, -0.1, -0.2, -0.3, 1, 2, 3, 0.1, 0.2, 0.3])
    inputData = adapter.prepareInputData(testData)
    print("agent's Position: " + str(adapter.agentPosition))
    print("agent's Orientation: " + str(adapter.agentOrientation))
    print("target's Position: " + str(adapter.targetPosition))
    print("target's Orientation: " + str(adapter.targetOrientation))

    print(str(inputData))

# ==============================================================================
if __name__ == "__main__":
    unitaryTests()

# end of file
# ==============================================================================

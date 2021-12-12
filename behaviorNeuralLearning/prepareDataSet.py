# ==============================================================================
# context: teaching - Shared Educational Resources in Computer Science
#          ENIB: module IAS - lab work on neural networks (since fall'18)
#                           - student project (fall'21)
# description: splits the learning data set into 3 subsets
#     namely, one for training, one for validation and the last one for testing
# copyright (c) 2018-2021 ENIB. All rights reserved.
# ------------------------------------------------------------------------------
# usage: python prepareDataSet.py <logFile> <learningDatafilePath>
# dependencies: 
#  - python 3 (see import statements)
#  - Structure of the log file produced by Unity (AgentsTransformLogger.cs)
# tested with:
# - python 3.8.5 on macOS 11.3 (from shell)
# - python 3.9.6 on windows 10 (from anaconda prompt within conda env) 
# ------------------------------------------------------------------------------
# creation: 29-jun-2018
# revision: 11-apr-2021 pierre.chevaillier@enib.fr decimal separator, eol, csv_sep
# revision: 06-sep-2021 pierre.chevaillier@enib.fr remove depend. to pandas, some code cleaning and commenting
# revision: 10-sep-2021 pierre.chevaillier@enib.fr MAJOR revision: add plot, change data format
# revision: 15-sep-2021 pierre.chevaillier@enib.fr FIX: load Unity log file either with comma or point as decimal separator
# ------------------------------------------------------------------------------
# comments:
#  - This version is dedicated to the project (nature of the learning data)
#    the algorithm remains the same.
# warnings:
# - only for educational purpose
# todos:
#  - critical: adapt to the problem (learning variables)
#  - improvement: more controls on the command line arguments
#  - add the possibility to scale the learning data 
#    (warning: should be consistent with the NN activation functions and neuralController.py)
# ==============================================================================

# Python standard distribution
import sys
import os, re

# Specific modules
import numpy as np

# Graphics
import matplotlib as mpl
import matplotlib.pyplot as plt
import mpl_toolkits as mplot3d

# Home made stuffs
from agentModelAdapter import clampToPiMinusPi
from agentModelAdapter import pointToPointDistance

# ==============================================================================
# Some global constants
eol = "\n" # SEE: https://docs.python.org/3/library/os.html (see os.linesep)
csv_sep = ';'
verbose = 1

# --- Ranges for the behavioral variables (sensors and actuators)
# Must be consistent with respect to the agent's controller.
# TODO: adapt to the problem
xDim = 4
yDim = 2

# Lower and Upper limites for herein calculated variables
# TODO: adapt to the problem
# Input (X):
distMin, distMax = .0, 20. # depends on the target positionning (see ChangePlaceRandomly.cs)
azimMin, azimMax = -3.15, 3.15 # a little bit more that -pi, pi

# output (Y)
vLinMin, vLinMax = .0, 1. # see KBVelocityController.cs
vAngMin, vAngMax = -1., 1. # more subtle because velocities are reralculated here

# ==============================================================================
# Locally defined functions

def prepareDataSet(pathToRawDataFile, pathToLearningDataFile):

    print("Load the data set (raw formatting) from " + pathToRawDataFile)
    columns = tuple(i for i in range(0, 18)) # WARNING: the two last columns may be empty, and thus should not be used 
    # WARNING: PORT: depending on locale setting, Unity may generate log files with , as decimal separator...
    rawData = np.loadtxt(pathToRawDataFile, dtype=float, encoding='ascii', delimiter = csv_sep, skiprows = 1, usecols=columns, 
        converters={i: lambda s: float(s.replace(',','.')) for i in columns})
    time = rawData[:,[0]]
    aPos = rawData[:,[1,2,3]]
    aOri = rawData[:,[5]]
    tPos = rawData[:,[7,8,9]]
    states = rawData[:,[17]]
    # TODO tOri ...
    
    nRecords = aPos.shape[0]
    print("Number of Records:", nRecords)

    if verbose > 0:
        headSize = 5
        print("Raw data structure: first " + str(headSize) + " of " + str(nRecords) + " records")
        print("Agent's position:\n" + str(aPos[range(headSize),:]))
        print("Agent's orientation:\n" + str(aOri[range(headSize),:]))
        print("target's position:\n" + str(tPos[range(headSize),:]))

    # --- Compute and saved the learning data
    X = np.zeros((nRecords-1, xDim), dtype= float)
    Y = np.zeros((nRecords-1, yDim), dtype= float)

    aLinearVelocity = 0.0
    aAngularVelocity = 0.0
    tLinearVelocity = 0.0
    for i in range(nRecords):
        j = i-1
        # Agent's orientation (direction of move in the horizontal plane)
        aPsi = clampToPiMinusPi(np.pi / 2 - aOri[i][0])
    
        # Position of the target, relative to the agent's position (in the horizontal plane)
        d = pointToPointDistance(tPos[i], aPos[i])
        if d > 0:
            tPosA = tPos[i] - aPos[i]
            azim = np.arctan2(tPosA[2], tPosA[0])
            theta = clampToPiMinusPi(aPsi - azim)
        else:
            theta = 0.0

        # estimated velocities of the agent
        # TODO: add the computation of the estimated velocity for the Target
        if i > 0:
            deltaT = time[i][0] - time[i-1][0]
            aLinearVelocity = pointToPointDistance(aPos[i], aPos[i-1]) / deltaT
  
            tLinearVelocity = pointToPointDistance(tPos[i], tPos[i-1]) / deltaT

            aPsi1, aPsi2 = aOri[i-1][0], aOri[i][0]
            dPsi = clampToPiMinusPi(aPsi2 - aPsi1)
            aAngularVelocity = dPsi / deltaT
            state = states[i]
            #print("Vang: " + str(aAngularVelocity) + ": " + str(aPsi2) + " - " + str(aPsi2) + " = " + str(dPsi) + " / " + str(deltaT))
            X[j,0], X[j,1], X[j,2], X[j,3] = d, theta, tLinearVelocity, state
            Y[j,0], Y[j,1] = aLinearVelocity, aAngularVelocity
    
    if verbose > 0:
        headSize = 5
        print("Calculated data structure: first " + str(headSize) + " of " + str(X.shape[0]) + " records")
        print("Distance to target:\n" + str(X[range(headSize),0]))
        print("Azimuth of the target:\n" + str(X[range(headSize),1]))
        print("Agent's linear velocity:\n" + str(Y[range(headSize),0]))
        print("Agent's angular velocity:\n" + str(Y[range(headSize),1]))
     
    # --- Print some stats on the data (optional)
    print("Stats on agent-target distance (m):" \
        + " min = " + str(np.amin(X[:,0])) 
        + ' max = ' + str(np.amax(X[:,0]))
        + ' mean = '  + str(np.mean(X[:,0]))
        + ' std dev = '  + str(np.std(X[:,0]))
        )
    print("Stats on target's azimuth (rad):" \
        + " min = " + str(np.amin(X[:,1])) 
        + ' max = ' + str(np.amax(X[:,1]))
        + ' mean = '  + str(np.mean(X[:,1]))
        + ' std dev = '  + str(np.std(X[:,1]))
        )
    print("Stats on agent's linear velocity (m.s-1):" \
        + " min = " + str(np.amin(Y[:,0])) 
        + ' max = ' + str(np.amax(Y[:,0]))
        +  ' mean = '  + str(np.mean(Y[:,0]))
        +  ' std dev = '  + str(np.std(Y[:,0]))
    )
    print("Stats on agent's angular velocity (rad.s-1):" \
        + " min = " + str(np.amin(Y[:,1])) 
        + ' max = ' + str(np.amax(Y[:,1]))
        +  ' mean = '  + str(np.mean(Y[:,1]))
        +  ' std dev = '  + str(np.std(Y[:,1]))
    )

    # --- Save the generated samples into the file
    Data = np.hstack((X, Y))
    np.savetxt(pathToLearningDataFile, Data, delimiter = csv_sep)
    print("Data have been stored in " + pathToLearningDataFile)

    # --- Plot to get an idea of the shape on the data set
    plotLearningData(X, Y, pathToLearningDataFile)
    return

def plotLearningData(X,Y, pathToLearningDataFile):
    pathToFigureDir = os.path.dirname(pathToLearningDataFile)
    fileNamePrefix = os.path.basename(pathToLearningDataFile)
    fileNamePrefix = re.split("\.", fileNamePrefix)[0]
    print(pathToFigureDir, fileNamePrefix)

    fig = plt.figure()
    ax = fig.suptitle("Learning data")
    ax = fig.add_subplot(projection='3d')
    ax.set_xlabel("distance to target (m)")
    ax.set_ylabel("target azimuth (rad)")
    ax.set_zlabel("Linear velocity (m.s-1)")
    ax.set_title(fileNamePrefix + ' data set')
    ax.scatter(X[:,0], X[:,1], Y[:,0])
    pathToFigureFile = os.path.join(pathToFigureDir, fileNamePrefix + '_linearVelo.png')
    print("Plot of sampled linear velocity saved into " + pathToFigureFile)
    plt.savefig(pathToFigureFile)

    fig = plt.figure()
    ax = fig.suptitle("Learning data")
    ax = fig.add_subplot(projection='3d')
    ax.set_xlabel("distance to target (m)")
    ax.set_ylabel("target azimuth (rad)")
    ax.set_zlabel("Angular velocity (rad.s-1)")
    ax.set_title(fileNamePrefix + ' data set')
    ax.scatter(X[:,0], X[:,1], Y[:,1])
    pathToFigureFile = os.path.join(pathToFigureDir, fileNamePrefix + '_angularVelo.png')
    print("Plot of sampled angular velocity saved into " + pathToFigureFile)
    plt.savefig(pathToFigureFile)

    return

# ==============================================================================
if __name__ == "__main__":
    pathToRawDataFile = sys.argv[1]
    pathToLearningDataFile = sys.argv[2]

    prepareDataSet(pathToRawDataFile, pathToLearningDataFile)

# end of file
# ==============================================================================

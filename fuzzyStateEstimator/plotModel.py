# creation: 06-dec-2020 pierre.chevaillier@enib.fr from existing file
# revision: 16-may-2021 pierre.chevaillier@enib.fr matplotlib >= 3.4

import sys, os, re
import numpy as np

import matplotlib as mpl
import matplotlib.pyplot as plt
from matplotlib import cm
import mpl_toolkits.mplot3d

def plotLawOfControl(dataFilePath, dirName, figsFileNamePrefix):

    print("Load data from " + dataFilePath)
    sourceFile = open(dataFilePath, "r")

    line = sourceFile.readline()
    tokens = re.split(";", line)
    distances = []
    for t in tokens:
        distances.append(float(t))
    nDistances = len(distances)

    line = sourceFile.readline()
    tokens = re.split(";", line)
    azimuths = []
    for t in tokens:
        azimuths.append(float(t))
    nAzimuths = len(azimuths)

    linearVelocities = []
    angularVelocities = []
    while (sourceFile):
        line = sourceFile.readline()
        if (len(line) == 0):
            break
        tokens = re.split(";", line)
        linearVelocities.append(float(tokens[0]))
        angularVelocities.append(float(tokens[1]))

   # nRecords = len(linearVelocities)

    xs = np.zeros([nAzimuths, nDistances])
    ys = np.zeros([nAzimuths, nDistances])
    vLins = np.zeros([nAzimuths, nDistances])
    vAngs = np.zeros([nAzimuths, nDistances])

    for i in range(0, nAzimuths):
        for j in range(0, nDistances):
            xs[i,j] = distances[j]
            ys[i,j] = azimuths[i]
            vLins[i,j] = linearVelocities[j  * nAzimuths + i]
            vAngs[i,j] = angularVelocities[j  * nAzimuths + i]
    print("Output files (figures)")
    fig = plt.figure()
    ax = plt.axes(projection='3d')
    ax.set_xlabel("distance to target (m)")
    ax.set_ylabel("azimuth of the target (rad)")
    ax.set_zlabel("Linear Velocity (m/s)")
    ax.plot_surface(xs, ys, vLins, rstride=1, cstride=1, cmap=plt.get_cmap('viridis'))
    figFileName = dirName + os.path.sep + figsFileNamePrefix + '_linearVelocity'
    print("\t - " + figFileName + ".png")
    plt.savefig(figFileName)

    fig = plt.figure()
    ax = plt.axes(projection='3d')
    ax.set_xlabel("distance to target (m)")
    ax.set_ylabel("azimuth of the target (rad)")
    ax.set_zlabel("Angular Velocity (rad/s)")
    ax.plot_surface(xs, ys, vAngs, rstride=1, cstride=1, cmap=plt.get_cmap('viridis'))
    figFileName = dirName + os.path.sep + figsFileNamePrefix + '_angularVelocity'
    print("\t - " + figFileName + ".png")
    plt.savefig(figFileName)
    return

if __name__ == "__main__":
    dataFilePath = sys.argv[1] + ".csv"
    if os.path.exists(dataFilePath):
        dataDir = os.path.dirname(dataFilePath)
        if dataDir == "": dataDir = "."
        print("data dir", dataDir)
    else:
        print("Error: data file " + dataFilePath + " does not exist.")
        sys.exit(1)
    
    figsFileNamePrefix = sys.argv[1]

    plotLawOfControl(dataFilePath, dataDir, figsFileNamePrefix)
# end fo file

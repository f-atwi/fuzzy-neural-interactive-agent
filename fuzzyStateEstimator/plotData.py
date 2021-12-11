# creation: 06-dec-2020 pierre.chevaillier@enib.fr from existing file
# revision: 16-may-2021 pierre.chevaillier@enib.fr matplotlib >= 3.4

import sys
import os
import re
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

    line = sourceFile.readline()
    tokens = re.split(";", line)
    velocities = []
    for t in tokens:
        velocities.append(float(t))
    nVelocities = len(velocities)

    rest = sourceFile.read()
    results = re.split("\n", rest)
    results.pop(-1)

    # results = [float(x)*100 for x in results]
    records = np.empty((0, 4))
    index = 0
    for distance in distances:
        for azimuth in azimuths:
            for velocity in velocities:
                if float(results[index])<9:
                    records = np.append(records, [[float(distance), float(azimuth), float(velocity), float(results[index])]], axis=0)
                index +=1
    print(records)
    print("Output files (figures)")
    fig = plt.figure()
    ax = plt.axes(projection='3d')
    ax.set_xlabel("distance to target (m)")
    ax.set_ylabel("azimuth of the target (rad)")
    ax.set_zlabel("Linear Velocity (m/s)")
    ax.scatter(records[:,0], records[:,1], records[:,2], c=records[:,3],  cmap=plt.get_cmap('viridis'))
    figFileName = figsFileNamePrefix + 'States'
    print("\t - " + figFileName + ".png")
    plt.savefig(figFileName)
    plt.show()
    return


if __name__ == "__main__":
    dataFilePath = sys.argv[1] + ".csv"
    if os.path.exists(dataFilePath):
        dataDir = os.path.dirname(dataFilePath)
        if dataDir == "":
            dataDir = "."
        print("data dir", dataDir)
    else:
        print("Error: data file " + dataFilePath + " does not exist.")
        sys.exit(1)

    figsFileNamePrefix = sys.argv[1]

    plotLawOfControl(dataFilePath, dataDir, figsFileNamePrefix)
# end fo file

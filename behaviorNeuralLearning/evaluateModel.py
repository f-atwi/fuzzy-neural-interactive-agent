# ==============================================================================
# context: teaching - Shared Educational Resources in Computer Science
#          ENIB: module IAS - Lab work on neural networks (since fall'21)
# description: evaluate the learning process 
# copyright (c) 2021 ENIB. All rights reserved.
# ------------------------------------------------------------------------------
# usage: python evaluateModel <pathToDataDir> <dataFilePrefix> <modelFilePrefix>
# dependencies: 
#  - python 3 (see import statements)
#  - files produced by other scripts: prepareDataSet, learnModel, splitDataSet (naming + structure)
# tested with:
# - python 3.8.5 on macOS 11.3 (from shell) 
# ------------------------------------------------------------------------------
# creation: 31-aug-2021 pierre.chevaillier@enib.fr
# revision: 09-sep-2021 pierre.chevaillier@enib.fr predict and plot
# ------------------------------------------------------------------------------
# comments:
# - 
# warnings:
# - only for educational purpose
# todos:
#  - tests on other platforms
# ==============================================================================

# Python standard distribution
import sys
import os

# PORT: decimal separator for savetxt / loadtxt <>
import locale
locale.getlocale()

# Specific modules
import numpy as np

# Graphics
import matplotlib as mpl
import matplotlib.pyplot as plt
from matplotlib import cm
import mpl_toolkits as mplot3d

# Machine Learning library
from keras.models import load_model

# Home made stuffs
# None

# ==============================================================================
# Global variables:
csv_sep = ';'

# TODO: adapt to the problem
distMin, distMax = .0, 20.
azimMin, azimMax = -3.15, 3.15
vLinMin, vLinMax = .0, 1.
vAngMin, vAngMax = -1., 1.

# ==============================================================================
# Locally defined functions

# picked from 
# # https://stackoverflow.com/questions/11144513/cartesian-product-of-x-and-y-array-points-into-single-array-of-2d-points
def cartesian_product(*arrays):
    la = len(arrays)
#    dtype = numpy.result_type(*arrays)
    arr = np.empty([len(a) for a in arrays] + [la], dtype=float)
    for i, a in enumerate(np.ix_(*arrays)):
        arr[...,i] = a
    return arr.reshape(-1, la)

# ------------------------------------------------------------------------------
def evaluateModel(pathToLearningDataDir, dataFileNamesPrefix, modelFileNamesPrefix):
    # the two following actions can be performed independently
    evaluateLearningProcess(pathToLearningDataDir, modelFileNamesPrefix)
    evaluatePredictions(pathToLearningDataDir, dataFileNamesPrefix, modelFileNamesPrefix)
    return

# ------------------------------------------------------------------------------
def evaluateLearningProcess(pathToLearningDataDir, modelFileNamesPrefix):
    # --- Read data
    lossesFileName = modelFileNamesPrefix + '_loss.csv'
    pathToDataFile = os.path.join(pathToLearningDataDir, lossesFileName)
    data = np.loadtxt(pathToDataFile, dtype = float, delimiter = csv_sep)
    LossesOnTrainingData, LossesOnValidationData = data[:,[0]], data[:,[1]]

    # ---- Plot the learning curves
    plt.figure()
    plt.plot(np.log10(LossesOnTrainingData))
    plt.plot(np.log10(LossesOnValidationData))
    plt.title('Learning curve for ' + modelFileNamesPrefix)
    plt.ylabel('loss (log10 scale)')
    plt.xlabel('epoch')
    plt.legend(['train', 'valid'], loc = 'upper left')
    pathToFigureFile = os.path.join(pathToLearningDataDir, modelFileNamesPrefix + '_loss.png')
    plt.savefig(pathToFigureFile)

    return

# ------------------------------------------------------------------------------
def evaluatePredictions(pathToLearningDataDir, dataFileNamesPrefix, modelFileNamesPrefix):

    # --- Load the data set
    pathToDataFile = os.path.join(pathToLearningDataDir, dataFileNamesPrefix + '_test.csv')
    data = np.loadtxt(pathToDataFile, dtype = float, delimiter = csv_sep)
    X, Y = data[:,[0,1]], data[:,[2,3]]

    # --- Load the model (the trained neural network)
    modelFileName = modelFileNamesPrefix + '_model.h5'
    pathToModelFile = os.path.join(pathToLearningDataDir, modelFileName)
    neuralNetwork = load_model(pathToModelFile)

    # --- Compute some metrics
    results= neuralNetwork.evaluate(X, Y)
    print("Loss on test data subset:", results)

    # --- Make predictions using the trained model and the test data set  
    YPred = neuralNetwork.predict(X)
    plotPredictions(X, YPred, pathToLearningDataDir, dataFileNamesPrefix  +'_test', modelFileNamesPrefix)

    computeAndPlotLearnedFunction(neuralNetwork, pathToLearningDataDir, dataFileNamesPrefix + '_grid', modelFileNamesPrefix)

    return

# ------------------------------------------------------------------------------
def plotPredictions(X, YPred, pathToDataDir, dataSetName, modelName):

    fig = plt.figure()
    ax = fig.suptitle('Model ' +  modelName)
    ax = fig.add_subplot(projection='3d')
    ax.set_xlabel("distance to target (m)")
    ax.set_ylabel("target azimuth (rad)")
    ax.set_zlabel("predict. Lin. velo. (m.s-1)")
    ax.set_title('Predictions for ' + dataSetName + ' data set')
    ax.scatter(X[:,0], X[:,1], YPred[:,0])
    pathToFigureFile = os.path.join(pathToDataDir, dataSetName + '_' + modelFileNamesPrefix + '_linearVelo.png')
    print("Plot of predicted linear velocity for " + dataSetName + " data set \nusing " + modelName + "\nsaved into " + pathToFigureFile)
    plt.savefig(pathToFigureFile)

    fig = plt.figure()
    ax = fig.suptitle('Model ' +  modelName)
    ax = fig.add_subplot(projection='3d')
    ax.set_xlabel("distance to target (m)")
    ax.set_ylabel("target azimuth (rad)")
    ax.set_zlabel("predict. Ang. velo. (rad.s-1)")
    ax.set_title('Predictions for ' + dataSetName + ' data set')
    ax.scatter(X[:,0], X[:,1], YPred[:,1])
    pathToFigureFile = os.path.join(pathToDataDir, dataSetName + '_' + modelFileNamesPrefix + '_angularVelo.png')
    print("Plot of predicted angular velocity for " + dataSetName + " data set \nusing " + modelName + "\nsaved into " + pathToFigureFile)
    plt.savefig(pathToFigureFile)

    return

# ------------------------------------------------------------------------------
def computeAndPlotLearnedFunction(model, pathToDataDir, dataSetName, modelName):

    # --- Define the input values (over the range of the input variables) 
    nValues = 21
    Distances = np.linspace(distMin, distMax, num=nValues, endpoint=True, dtype=float)
    Azimuthes = np.linspace(azimMin, azimMax, num=nValues, endpoint=True, dtype=float)

    # --- Predict the corresponding output values using the  trained model 
    X = cartesian_product(*[Distances, Azimuthes])
    YPlot = model.predict(X)

    # --- Strucutre the data for plotting
    X1, X2 = np.meshgrid(Distances, Azimuthes)
    Y1 = np.zeros([nValues, nValues])
    Y2 = np.zeros([nValues, nValues])
    for i in range(nValues):
        for j in range(nValues):
            k = j + nValues * i
            Y1[j,i], Y2[j,i] = YPlot[k, 0], YPlot[k, 1] 

    # --- Plotting 
    # Linear velocity
    fig = plt.figure()
    ax = fig.suptitle('Model ' +  modelName)
    ax = fig.add_subplot(projection='3d')
    ax.set_xlabel("distance to target (m)")
    ax.set_ylabel("target azimuth (rad)")
    ax.set_zlabel("predict. Lin. velo. (m.s-1)")
    ax.set_title('Predictions for ' + dataSetName + ' data set')
    ax.plot_surface(X1, X2, Y1, cmap=plt.get_cmap('viridis'))
    pathToFigureFile = os.path.join(pathToDataDir, dataSetName + '_' + modelFileNamesPrefix + '_linearVelo.png')
    print("Plot linear velocities for " + dataSetName + " data set \nusing " + modelName + "\nsaved into " + pathToFigureFile)
    plt.savefig(pathToFigureFile)

    # Angular velocity
    fig = plt.figure()
    ax = fig.suptitle('Model ' +  modelName)
    ax = fig.add_subplot(projection='3d')
    ax.set_xlabel("distance to target (m)")
    ax.set_ylabel("target azimuth (rad)")
    ax.set_zlabel("predict. Ang. velo. (rad.s-1)")
    ax.set_title('Predictions for ' + dataSetName + ' data set')
    ax.plot_surface(X1, X2, Y2, cmap=plt.get_cmap('viridis'))
    pathToFigureFile = os.path.join(pathToDataDir, dataSetName + '_' + modelFileNamesPrefix + '_angularVelo.png')
    print("Plot angular velocities for " + dataSetName + " data set \nusing " + modelName + "\nsaved into " + pathToFigureFile)
    plt.savefig(pathToFigureFile)
    return

# ==============================================================================
if __name__ == "__main__":

    # Where to read and write the different files
    # retrieve the information provided from the command line
    iArg = 1
    if len(sys.argv) > iArg:
        pathToLearningDataDir = sys.argv[iArg]
    else:
        pathToLearningDataDir = '.'
    if not os.path.isdir(pathToLearningDataDir):
        print("Error: directory " + pathToLearningDataDir + " does not exist.")
        sys.exit(iArg)
    
    iArg = 2
    if len(sys.argv) > iArg:
        dataFileNamesPrefix = sys.argv[iArg]
    else:
        print("Error: provide the prefix of the learning data files' name")
        sys.exit(iArg)

    iArg = 3
    if len(sys.argv) > iArg:
        modelFileNamesPrefix = sys.argv[iArg]
    else:
        print("Error: provide the prefix of the model files' name")
        sys.exit(iArg)

    evaluateModel(pathToLearningDataDir, dataFileNamesPrefix, modelFileNamesPrefix)

# end of file
# ==============================================================================

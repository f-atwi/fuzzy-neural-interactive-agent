# ==============================================================================
# context: teaching - Shared Educational Resources in Computer Science
#          ENIB: module IAS - lab work on neural networks (since fall'18)
#                           - student project (fall'21)
# description: splits the learning data set into 3 subsets
#     namely, one for training, one for validation and the last one for testing
# copyright (c) 2018-2021 ENIB. All rights reserved.
# ------------------------------------------------------------------------------
# usage: python splitDataSet <filename>
#     <filename>: the path to the file containing the whole data set
#     it must be a csv file (1 record = 1 sample)
# dependencies: python 3 (see import statements)
# tested:
#  - with python 3.8.x on MacOS 11.6
# ------------------------------------------------------------------------------
# creation: 31-aug-2018 pierre.chevaillier@enib.fr
# revision: 17-sep-2018 pierre.chevaillier@enib.fr code cleaning
# ------------------------------------------------------------------------------
# comments:
# - not sensitive to the dimension of the input/output variable
# - students have to set the proportions of data for the different subsets
#   see 'TODO' tags
# warnings:
# - for educational purpose only
# todos:
#  - more controls on the command line arguments
#  - percentage to be passed through the command line
# ==============================================================================

# Python standard distribution
import sys, os, re

# Specific modules
import numpy as np

# Home made stuffs

# Some global constants
eol = "\n" # SEE: https://docs.python.org/3/library/os.html (see os.linesep)
csv_sep = ';'

# -----------------------------------------------------------------------------
def splitData(learningDataPath, dirName, props, shuffle):

    print("Load the data from " + learningDataPath)
    data = np.loadtxt(learningDataPath, dtype=float, delimiter= csv_sep)

    if shuffle:
        np.random.shuffle(data)

    fileNamePrefix = os.path.basename(learningDataPath)
    fileNamePrefix = re.split("\.", fileNamePrefix)[0]

    nData = len(data)
    nValid = nData * props[0] // 100
    nTests = nData *  props[1] // 100
    nTrain = nData - (nValid + nTests)

    print("The whole data set contains " + str(nData) + " samples")
    trainData = []
    if nTrain > 1:
        first = 0
        last = nTrain
        trainData = data[first:last]
    else:
        print("Error: according to the proportions you've set, the training data subset would be empty...")
        sys.exit(1)

    print("Number of samples in the training data set: " + str(len(trainData)))
    fileName = os.path.join(dirName, fileNamePrefix + '_training' + '.csv')
    np.savetxt(fileName, trainData, delimiter = csv_sep, newline = eol)
    print("\tsaved into " + fileName)

    validData = []
    if nValid > 1:
        first = nTrain
        last = first + nValid
        validData = data[first:last]
    print(" Number of samples in the validation data set: " + str(len(validData)) + " / " + str(nData))
    fileName = os.path.join(dirName, fileNamePrefix + '_validation' + '.csv')
    np.savetxt(fileName, validData, delimiter = csv_sep, newline = eol)
    print("\tsaved into " + fileName)

    testData = []
    if nTests > 1:
        first = nTrain + nValid
        last = first + nTests
        testData = data[first:last]
    print(" Number of samples in the test data set: " + str(len(testData)) + " / " + str(nData))
    fileName = os.path.join(dirName, fileNamePrefix + '_test' + '.csv')
    np.savetxt(fileName, testData, delimiter = csv_sep, newline = eol)
    print("\tsaved into " + fileName)
    return

# ==============================================================================
if __name__ == "__main__":
    learningDataPath = sys.argv[1]
    if os.path.exists(learningDataPath):
        dataDir = os.path.dirname(learningDataPath)
    else:
        print("Error: learning data file " + learningDataPath + " does not exist.")
        sys.exit(1)

    # TODO: set the two following values
    propValid = 30 # percentage of the samples for the validation data subset
    propTest = 20 # percentage of the samples for the test data subset

    # Therefore, the percentage of the samples actually used for the training 
    # equals 100 - (propValid + propTest)
    validTestProps = [propValid,propTest]

    shuffle = True # statically safe
    splitData(learningDataPath, dataDir, validTestProps, shuffle)

# end of file
# ==============================================================================

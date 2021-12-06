# creation: 23-aug-2018 pierre.chevaillier@enib.fr
# revision: 06-sep-2021 pierre.chevaillier@enib.fr 
# ------------------------------------------------------------------------------
# comments:
# - 
# warnings:
# - only for educational purpose
# todos:
#  - tests on other platforms
# ==============================================================================

# Python standard distribution
import sys, getopt
from os import path

# Specific modules
import numpy as np

# Machine Learning library  
from keras.models import load_model

# ==============================================================================
# Locally defined classes

class NeuralVelocityController:
    def __init__(self):
        #self.architectureFilename = ''
        #self.weightsFileName = ''
        #self.seqLength = 1
        self.pathToModelFile = ''

    def configure (self, opts, args):
        print(str(opts))
        for opt, arg in opts:
            if opt in ("-m", "--model"):
                self.pathToModelFile = arg

        if not path.exists(self.pathToModelFile):
            print("Error: Model file " 
                + self.pathToModelFile 
                + " does not exist...")
            sys.exit()
  
        print("Trained model loaded from " + self.pathToModelFile)
        return

    def build(self):
        self._model =load_model(self.pathToModelFile)
        return

    def process(self, features):
        return (self._model.predict(features))[0]


def parseCommandLine():
    try:
        opts, args = getopt.getopt(sys.argv[1:], "hm:v", ["help", "model="])
    except getopt.GetoptError as err:
        print(str(err))
        sys.exit(2)
    return opts, args

# ==============================================================================
def unitaryTests():
    opts, args = parseCommandLine()

    neuralController = NeuralVelocityController()
    neuralController.configure(opts, args)
    neuralController.build()

    feature = [0.5, 0.5]
    sample = np.array([feature])
    print(str(neuralController.process(sample)))

# ==============================================================================
if __name__ == "__main__":
    unitaryTests()

# end of file
# ==============================================================================

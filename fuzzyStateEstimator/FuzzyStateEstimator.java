// =============================================================================
// context: teaching - Shared Educational Resources in Computer Science
//          ENIB: module IAS - students' project
// description:
// copyright (c) 2021 ENIB. All rights reserved.
// -----------------------------------------------------------------------------
// usage: java fuzzyController.Surface
// dependencies:
//   - jFuzzyLogic.jar (needs to be reachable)
//   - names of the linguistic variables defined in the FCL file
// tested:
//  - with java 1.8 on on MacOS 11.6
// -----------------------------------------------------------------------------
// creation: 15-may-2019 pierre.chevaillier@enib.fr
// revision: 06-dec-2020 pierre.chevaillier@enib.fr 1st version from existing file
// -----------------------------------------------------------------------------
// comments:
// warnings:
//  - for educational purposes only
// todos:
//  - test it and then test it again
// =============================================================================
package fuzzyStateEstimator;

// java stuff
import java.io.PrintWriter;
import java.io.FileNotFoundException;

// jFuzzyLogic stuff
import net.sourceforge.jFuzzyLogic.FIS;
import net.sourceforge.jFuzzyLogic.FunctionBlock;
import net.sourceforge.jFuzzyLogic.rule.Variable;

// -----------------------------------------------------------------------------
public class FuzzyStateEstimator {
  public FIS fis;
  public String basePathName;
  
  // imput linguistis variables
  public Variable linearVelocity; 
  public Variable distance;
  public Variable direction;
  // output linguistic variables
  public Variable mood;
  
  public boolean build() {
    boolean ok = true;
    String fuzzyControllerPathName =  this.basePathName + ".fcl";
    this.fis = FIS.load(fuzzyControllerPathName, true);
    if (this.fis == null) {
      System.err.println("Error: cannot load file: " + fuzzyControllerPathName);
      ok = false;
    } else {
      System.out.println("Loaded file: " + fuzzyControllerPathName);
      FunctionBlock block = fis.getFunctionBlock(null);
      if (block == null) System.err.println("Error: cannot retrieve the block");
      
      // --- retrieve linguistic variables
      this.linearVelocity = block.getVariable("linearVelocity");
      if (this.linearVelocity == null) {
        System.err.println("Error: cannot retrieve variable linearVelocity");
        ok = false;
      }
      this.distance = block.getVariable("distance");
      if (this.distance == null) {
          System.err.println("Error: cannot retrieve variable distance");
          ok = ok && false;
      }
      this.direction = block.getVariable("direction");
      if (this.direction == null) {
          System.err.println("Error: cannot retrieve variable direction");
          ok = ok && false;
      }
      this.mood = block.getVariable("mood");
      if (this.mood == null) {
          System.err.println("Error: cannot retrieve variable mood");
          ok = ok && false;
      }
    }
    return ok;
  }

  public boolean process(double[] input, double[] output) {
    this.linearVelocity.setValue(input[0]);
    this.distance.setValue(input[1]);
    this.direction.setValue(input[2]);
    this.fis.evaluate();
    output[0] = this.mood.getValue();
    return true;
  }

  public boolean evaluateForAll() {
    boolean ok = true;
    // Where to store the results
    String resultFileName = this.basePathName + ".csv";
    
    try {
      PrintWriter resultFile = new PrintWriter(resultFileName);
      System.out.println("Writing results into " + resultFileName);
      // ----------------------------------------------------------------------
      double distMin = 0.0, distMax = 20.0, dDist = .5;
      double dirMin = 0.0, dirMax = 3.14, dDir = 3.14/21.0;
      double vMin = 0.0, vMax = 2.0, dV = .1;
      double distance = distMin, direction = dirMin, velocity = vMin;

      // First lines: values of the imput variables

      while (distance < distMax) {
        resultFile.write(distance + ";");
        distance += dDist;
      }
      resultFile.println(distMax);

      while (direction < dirMax) {
        resultFile.write(direction + ";");
        direction += dDir;
      }
      resultFile.println(dirMax);

      while (velocity < vMax) {
        resultFile.write(velocity + ";");
        velocity += dV;
      }
      resultFile.println(vMax);

      // Next : corresponding values for output variables
      double[] input = new double[3];
      double[] output = new double[1];
      
      
      distance = distMin;
      while (distance < distMax + dDist) {
        direction = dirMin;
        while (direction < dirMax + dDir) {
          velocity = vMin;
          while (velocity < vMax + dV) {
            input[0] = velocity; input[1] = distance; input[2] = direction;
            this.process(input, output);
            resultFile.println(output[0]);
            velocity += dV;
          }
          direction += dDir;
        }
        distance += dDist;
      }
      
      // ----------------------------------------------------------------------
      resultFile.flush();
      resultFile.close();
    } catch (FileNotFoundException e) {
      System.out.println("Cannot write into file" + resultFileName);
      return false;
    }
    return ok;
  }
  
  public static void main(String[] args) throws Exception {
    String basePathName;
    if (args.length > 0) {
      basePathName = args[0];
    } else {
      System.err.println("Error: you must provide the base name of the FCL as the first args");
      return;
    }
    String mode = "all";
    int nVars = args.length - 1;
    double[] values = new double[nVars];
    
    if (args.length > 1) {
      mode = "one";
      for (int i = 0; i < nVars; i++)
        values[i] = Double.parseDouble(args[i+1]);
    }
    
    FuzzyStateEstimator stateEstimator = new FuzzyStateEstimator();
    stateEstimator.basePathName = basePathName;
    boolean status = false;
    status = stateEstimator.build();
    
    if (status) {
      if (mode.equals("all")) {
        status = stateEstimator.evaluateForAll();
     } else {
       double[] output = new double[1];
       status = stateEstimator.process(values, output);
       System.out.println("Mood index: " + output[0]);
      }
    } else {
      System.err.println("Error: failed to build the Fuzzy state estimator");
    }
    return;
  }
}
// end of file
//=============================================================================

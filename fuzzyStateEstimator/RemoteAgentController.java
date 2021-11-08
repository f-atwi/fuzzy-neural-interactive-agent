// =============================================================================
// context: teaching - Shared Educational Resources in Computer Science
//          ENIB: module IAS - fuzzy logic (since spring'19)
// description:
// copyright (c) 2019-2020 ENIB. All rights reserved.
// -----------------------------------------------------------------------------
// usage: java remoteController.RemoteAgentController <fuzzy-rules-file-prefix> 
// dependencies:  FuzzyBehaviorController
// tested on: java on MacOS 10.14
// -----------------------------------------------------------------------------
// creation: 16-may-2019 pierre.chevaillier@enib.fr (inception)
// revision: 03-may-2020 pierre.chevaillier@enib.fr
// -----------------------------------------------------------------------------
// comments:
// warnings:
//  - for educational purposes only
// todos:
//  - test it and then test it again
// =============================================================================
package remoteController;

// java stuff
import java.io.*;
import java.net.*;
import java.nio.ByteBuffer;
import java.nio.ByteOrder;

// ============================================================================
public class RemoteAgentController {
  
  public final static int readingPort = 5005;
  public final static int writtingPort = 5006;

  public static void main(String[] args) throws Exception {
    
    String basePathName;
    if (args.length > 0) {
      basePathName = args[0];
    } else {
      System.err.println("Error: you must provide the base name of the FCL as the first args");
      return;
    }
    
    // --- Build the Fuzzy Controller
    FuzzyBehaviorController fuzzyController = new FuzzyBehaviorController();
    fuzzyController.basePathName = basePathName;
    fuzzyController.build();
    
    // --- Defines the model adapter
    //     that convert raw data sent from the agent
    //     to value used by the controller
    AgentModelAdapter modelAdapter = new AgentModelAdapter();
    
    // --- Intialization for the communication with the agent's host
    final InetAddress server = InetAddress.getByName("localhost");
    final int sizeOfFloat = 4;
    
    // --- Incoming data ------------------------------------------------------
    //     floats stored as bytes
    final int nValues = 12;
    final int inputBufferSize = nValues * sizeOfFloat;
    byte[] inputRawData;
    float[] values = new float[nValues];
    ByteBuffer inputByteBuffer;
    DatagramPacket incomingPacket = new DatagramPacket(new byte[inputBufferSize],
                                                       inputBufferSize);
    DatagramSocket read_sock = new DatagramSocket(readingPort);
    
    // --- Processed data -----------------------------------------------------
    //     by the behavior controller
    double input[] = new double[2];
    double output[] = new double[2];
    
    // --- Outgoing data ------------------------------------------------------
    //     floats stored as bytes
    final int nResults = 2;
    final int outputBufferSize = nResults * sizeOfFloat;
    float[] results = new float[nResults];
    byte[] outputRawData;
    ByteBuffer outputByteBuffer;
    DatagramSocket write_sock = new DatagramSocket();

    long n = 0;
    System.out.println("Waiting for incoming data ...");
    while (true) {
      
      // --- Read incoming data sent by the agent (its current state)
      read_sock.receive(incomingPacket); // blocking read
      inputRawData = incomingPacket.getData();
      inputByteBuffer = ByteBuffer.wrap(inputRawData, 0, inputBufferSize);
      inputByteBuffer.order(ByteOrder.LITTLE_ENDIAN);
      inputByteBuffer.asFloatBuffer().get(values);
      
      // --- Process current data (use the local controller)
      input = modelAdapter.prepareInputData(values);
      n++;
      System.out.println("iter: " + n + " input " + input[0] + " - " + input[1]);
      fuzzyController.process(input, output);
      
      // --- Send computed results back to the agent
      for (int i = 0; i < nResults; i++) results[i] = (float)output[i];
      System.out.println("output " + results[0] + " - " + results[1]);
      
      outputByteBuffer = ByteBuffer.allocate(outputBufferSize);
      outputByteBuffer.order(ByteOrder.LITTLE_ENDIAN);
      outputByteBuffer.asFloatBuffer().put(results);
      outputRawData = outputByteBuffer.array();
      DatagramPacket outPacket = new DatagramPacket(outputRawData,
                                                    outputBufferSize,
                                                    server, writtingPort);
      write_sock.send(outPacket);
      
     }
  }
}
// end of file
//=============================================================================

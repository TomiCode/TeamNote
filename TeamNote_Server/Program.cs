using System;

using TeamNote.Server;

namespace TeamNote
{
  class Program
  {
    private static ServerInstance serverInstance;

    static void Main(string[] args)
    {
      Debug.Setup("TeamNote_Server.log");

      serverInstance = new ServerInstance();
      serverInstance.GenerateServerKeypair();
      serverInstance.Start();

      Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs e) => {
        serverInstance.Stop();
      };

      while(true) Console.ReadKey(true);
    }
  }
}

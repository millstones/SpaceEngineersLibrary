using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    class SharedOS
    {
        static SharedOS INSTANCE;
        
        public static void Init(IMyGridTerminalSystem gts)
        {
            var pbs = new List<IMyProgrammableBlock>();
            gts.GetBlocksOfType(pbs, block => block.CustomName.Contains(SharedOSConfig.PG_SHARED_OS_MARK));

            INSTANCE = new SharedOS();
        }

        public void Tick()
        {
            
        }

        public void Save()
        {
            
        }
    }

    class MasterSlaveChannel
    {
        IMyProgrammableBlock _master;

        public MasterSlaveChannel(IMyProgrammableBlock master)
        {
            _master = master;
        }

        MasterSlaveChannel AddSlave(params IMyProgrammableBlock[] slaves)
        {
            
        }
    }
}
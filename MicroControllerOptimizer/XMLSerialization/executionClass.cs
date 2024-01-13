using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MicroControllerOptimizer.XMLSerialization
{
    public class executionClass
    {
        public static executionClass GenerateRandomExecution()
        {
            var execution = new executionClass();

            execution.CreateRandomInteger();
            execution.CreateRandomFloating();
            execution.CreateRandomBranch();
            execution.CreateRandomMemory();

            return execution;
        }

        public void CreateMutatedInteger(double distance)
        {
            var movement = (int)Math.Round(distance * 8, MidpointRounding.ToPositiveInfinity);
            if (movement == 0)
                movement = 1;
            movement *= randomSign();
            if (integer + movement > 8 || integer + movement < 1)
                movement = -movement;
            integer += movement;
        }

        public void CreateMutatedFloating(double distance)
        {
            var movement = (int)Math.Round(distance * 8, MidpointRounding.ToPositiveInfinity);
            if (movement == 0)
                movement = 1;
            movement *= randomSign();
            if (floating + movement > 8 || floating + movement < 1)
                movement = -movement;
            floating += movement;
        }

        public void CreateMutatedBranch(double distance)
        {
            var movement = (int)Math.Round(distance * 8, MidpointRounding.ToPositiveInfinity);
            if (movement == 0)
                movement = 1;
            movement *= randomSign();
            if (branch + movement > 8 || branch + movement < 1)
                movement = -movement;
            branch += movement;
        }

        public void CreateMutatedMemory(double distance)
        {
            var movement = (int)Math.Round(distance * 8, MidpointRounding.ToPositiveInfinity);
            if (movement == 0)
                movement = 1;
            movement *= randomSign();
            if (memory + movement > 8 || memory + movement < 1)
                movement = -movement;
            memory += movement;
        }

        private int randomSign()
        {
            var rand = new Random();
            return (int)Math.Pow(-1, rand.Next(1, 2));
        }

        [XmlAttribute]
        public string architecture = "standard";
        [XmlAttribute]
        public int integer;
        public void CreateRandomInteger()
        {
            Random integer = new Random(DateTime.Now.TimeOfDay.Nanoseconds);
            this.integer = integer.Next(1, 9);
        }
        [XmlAttribute]
        public int floating;
        void CreateRandomFloating()
        {
            Random floating = new Random(DateTime.Now.TimeOfDay.Nanoseconds);
            this.floating = floating.Next(1, 9);
        }
        [XmlAttribute]
        public int branch;
        void CreateRandomBranch()
        {
            Random branch = new Random(DateTime.Now.TimeOfDay.Nanoseconds);
            this.branch = branch.Next(1, 9);
        }
        [XmlAttribute]
        public int memory;
        void CreateRandomMemory()
        {
            Random memory = new Random(DateTime.Now.TimeOfDay.Nanoseconds);
            this.memory = memory.Next(1, 9);
        }

        public executionClass DeepCopy()
        {
            var copiedExecution = this.MemberwiseClone() as executionClass;
            return copiedExecution;
        }
    }
}

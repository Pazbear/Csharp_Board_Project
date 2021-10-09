using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace project
{
    class QueueBuff
    {
        public byte[] buff;
        int QUEUEBUFSIZE = 1024;

        public int head = 0;
        public int tail = 0;
        public QueueBuff()
        {
            buff = new byte[QUEUEBUFSIZE];

        }
        public void Enqueue(byte[] data, int dataSize)
        {
            for(int i=0; i<dataSize; i++)
            {
                buff[head]=data[i];
                head = (head + 1) % QUEUEBUFSIZE;
            }
        }
    }
}

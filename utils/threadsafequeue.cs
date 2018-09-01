/*

Copyright 2014-2018 Daniel Kinsman.

This file is part of Stand Alone Map View.

Stand Alone Map View is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Stand Alone Map View is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Stand Alone Map View.  If not, see <http://www.gnu.org/licenses/>.

*/
using System.Collections.Generic;

namespace StandAloneMapView.utils
{
    // Would love to use System.Collections.Concurrent.ConcurrentQueue but our target is .net/mono 3.5
    public class ThreadSafeQueue<T>
    {
        protected Queue<T> queue;
        protected object queueLock = new object();

        public ThreadSafeQueue()
        {
            this.queue = new Queue<T>();
        }

        public void Push(T item)
        {
            lock(this.queueLock)
                this.queue.Enqueue(item);
        }

        public T Pop()
        {
            lock(this.queueLock)
                return this.queue.Dequeue();
        }

        // returns default if queue is empty
        public T TryPop(T emptyQueueValue)
        {
            lock(this.queueLock)
            {
                if(this.queue.Count > 0)
                    return Pop();
                else
                    return emptyQueueValue;
            }
        }
    }
}

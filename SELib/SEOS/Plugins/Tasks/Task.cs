using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace IngameScript
{
    class Task : TaskBase
    {
        Action onNext;
        Func<bool> where;

        public override bool MoveNext()
        {
            if (!MoveNextInternal())
                return false;
            if (where == null)
                onNext?.Invoke();
            else
            if (where())
                onNext?.Invoke();
                
            return true;
        }

        public Task Where(Func<bool> cmp)
        {
            where = cmp;
            return this;
        }

        public Task Do(Action act)
        {
            onNext += act;
            return this;
        }
        public static Task EveryTick(Action action)
        {
            return new Task()
                .Do(action);
        }

        public static Task<int> Delay(int ms)
        {
            var startDate = DateTime.MinValue;
            return new Task<int>(() => 
            {
                var thisDate = DateTime.Now;
                if (startDate == DateTime.MinValue)
                    startDate = thisDate;
                return (int)(thisDate - startDate).TotalMilliseconds;
            })
                .While((i) => i < ms);
        }

        public static Task<int> Repeat(int interval_ms, Action action, int count = -1)
        {
            var startDate = DateTime.MinValue;
            var counter = 0;
            var lastValueCounter = 0;

            return new Task<int>(() =>
            {
                var thisDate = DateTime.Now;
                if (startDate == DateTime.MinValue)
                    startDate = thisDate;

                var lastInterval = (int)(thisDate - startDate).TotalMilliseconds;
                if (lastInterval > interval_ms)
                {
                    counter++;
                    startDate = thisDate;
                }
                return counter;
            })
                .Do((i) => { action(); })
                .Where((i) =>
                {
                    if (lastValueCounter != i)
                    {
                        lastValueCounter = counter;
                        return true;
                    }

                    return false;
                })
                .While((i) => 
                {
                    if (count < 0)
                        return true;

                    return i < count;
                })
                ;
        }

        public static Task<TValue> ValueChanged<TObject, TValue>(TObject @object, Func<TObject, TValue> propSelector)
        {
            var lastValue = propSelector(@object);

            return new Task<TValue>(() => propSelector(@object))
                .Where((v) =>
                {
                    if (!v.Equals(lastValue))
                    {
                        lastValue = v;
                        return true;
                    }

                    return false;
                });
        }

        public static Task Trigger(Func<bool> trigger)
        {
            return new Task()
                .Where(trigger)
                //.While(trigger)
                //.OnComplete(action)
                ;
        }
    }

    class Task<T> : TaskBase, IEnumerator<T>
    {
        Action<T> onNext;
        Func<T> get;
        T _current;
        Func<T, bool> @while;
        Func<T, bool> where;

        T IEnumerator<T>.Current => _current;

        public Task(Func<T> get)
        {
            this.get = get;
        }

        public override bool MoveNext()
        {
            if (!MoveNextInternal())
                return false;

            _current = get();
            if (@while != null && @while(_current))
            {
                if (where != null && where(_current))
                   onNext?.Invoke(_current);
                return true;
            }
            return false;
        }
        public Task<T> Do(Action<T> act)
        {
            onNext += act;
            return this;
        }

        public void Dispose()
        {
            
        }
        public Task<T> While(Func<T, bool> cmp)
        {
            @while = cmp;
            return this;
        }
        public Task<T> Where(Func<T, bool> cmp)
        {
            where = cmp;
            return this;
        }
    }
}

using System;
using System.Collections;

namespace IngameScript
{
    abstract class TaskBase : IEnumerator
    {
        public TaskStatus Status { get; protected set; } = TaskStatus.Created;

        Action<Exception> onError;
        Action onComplete;
        Func<bool> @while;
        public object Current => null;

        public abstract bool MoveNext();
        protected bool MoveNextInternal()
        {
            if (Status == TaskStatus.Canceled ||
                Status == TaskStatus.Faulted ||
                Status == TaskStatus.RanToCompletion ||
                (@while != null && @while() == false))
                return false;

            Status = TaskStatus.Running;
            return true;
        }
        public void Reset()
        {
            Status = TaskStatus.WaitingToRun;
        }
        public void Cancel()
        {
            Status = TaskStatus.Canceled;
            onError?.Invoke(new Exception("Задача была отменена"));
        }

        public void Complete()
        {
            Status = TaskStatus.RanToCompletion;
            onComplete?.Invoke();
        }

        public void Error(Exception e)
        {
            Status = TaskStatus.Faulted;
            onError?.Invoke(e);
        }

        public TaskBase OnError(Action<Exception> act)
        {
            onError += act;
            return this;
        }
        public TaskBase OnComplete(Action act)
        {
            onComplete += act;
            return this;
        }
        public TaskBase While(Func<bool> cmp)
        {
            @while = cmp;
            return this;
        }
        public TaskBase Until(Func<bool> cmp)
        {
            @while = () => !cmp();
            return this;
        }
        
        public static TaskBase CompletedTask => new Task(){Status = TaskStatus.RanToCompletion};
        
        public static TaskBase AndCombineTask(params TaskBase[] tasks)
        {
            return new Task()
                .Do(() =>
                {
                    foreach (var task in tasks)
                    {
                        task.MoveNext();
                    }
                })
                .While(() =>
                {
                    var completed = 0;

                    foreach (var task in tasks)
                    {
                        completed += task.Status == TaskStatus.RanToCompletion? 1 : 0;
                    }

                    return completed == tasks.Length;
                });
        }
        public static TaskBase OrCombineTask(params TaskBase[] tasks)
        {
            return new Task()
                .Do(() =>
                {
                    foreach (var task in tasks)
                    {
                        task.MoveNext();
                    }
                })
                .While(() =>
                {
                    var completed = 0;

                    foreach (var task in tasks)
                    {
                        completed += task.Status == TaskStatus.RanToCompletion? 1 : 0;
                    }

                    return completed > 0;
                });
        }
    }
}
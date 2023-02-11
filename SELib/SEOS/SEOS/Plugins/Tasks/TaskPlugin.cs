using System;
using System.Collections.Generic;
using System.Linq;

namespace IngameScript
{
    class TaskPlugin : Plugin
    {
        struct TaskItm
        {
            public int Id;
            public CancellationToken token;
            public TaskBase enumerator;

            public TaskItm(TaskBase e, CancellationToken t)
            {
                token = t ?? new CancellationToken();
                enumerator = e;
                Id = enumerator.GetHashCode();
            }
        }

        static TaskPlugin INSTANCE;

        List<TaskItm> enumerators = new List<TaskItm>();
        double _lastDt;

        public override void Init(SEOS os)
        {
            base.Init(os);
            INSTANCE = this;
        }

        public override void Tick(double dt)
        {
            _lastDt = dt;
            var e = enumerators.ToArray();
            for (var i = 0; i < e.Length; i++)
            {
                var task = e[i];
                var taskNumber = i + 1;
                if (task.token.Exception != null)
                {
                    task.enumerator.Error(task.token.Exception);
                    Logger.Log(NoteLevel.Waring, $"Task {taskNumber} error: <<{task.token.Exception.Message}>>");
                }
                else if (task.token.CancelRequest)
                {
                    task.enumerator.Cancel();
                }
                else if (task.token.CompleteRequest || task.enumerator.MoveNext() == false)
                {
                    task.enumerator.Complete();
                }
            }

            var remList = e.Where(itm => itm.token.Exception != null ||
                                        itm.token.CancelRequest ||
                                        itm.token.CompleteRequest ||

                                        itm.enumerator.Status == TaskStatus.Canceled ||
                                        itm.enumerator.Status == TaskStatus.Faulted ||
                                        itm.enumerator.Status == TaskStatus.RanToCompletion
            );

            foreach (var itm in remList)
            {
                var i = enumerators.Find(x => x.Id == itm.Id);
                enumerators.Remove(i);
            }

            Logger.Log($"Task plugin: 'Running {enumerators.Count} tasks.'");
        }

        static void ThrowIfNotCreate()
        {
            if (INSTANCE == null)
                throw new Exception("SEEnumerator plugin not create");
        }
        public static void Run(TaskBase enumerator, CancellationToken token)
        {
            ThrowIfNotCreate();

            INSTANCE.enumerators.Add(new TaskItm(enumerator, token));
        }
    }
}
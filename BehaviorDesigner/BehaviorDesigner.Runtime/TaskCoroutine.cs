using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
namespace BehaviorDesigner.Runtime
{
	public class TaskCoroutine
	{
		private IEnumerator mCoroutineEnumerator;
		private Coroutine mCoroutine;
		private Behavior mParent;
		private string mCoroutineName;
		private bool mStop;
		public Coroutine Coroutine
		{
			get
			{
				return this.mCoroutine;
			}
		}
		public TaskCoroutine(Behavior parent, IEnumerator coroutine, string coroutineName)
		{
			this.mParent = parent;
			this.mCoroutineEnumerator = coroutine;
			this.mCoroutineName = coroutineName;
			this.mCoroutine = parent.StartCoroutine(this.RunCoroutine());
		}
		public void Stop()
		{
			this.mStop = true;
		}
		[DebuggerHidden]
		public IEnumerator RunCoroutine()
		{
            //TaskCoroutine.<RunCoroutine>c__Iterator1 <RunCoroutine>c__Iterator = new TaskCoroutine.<RunCoroutine>c__Iterator1();
            //<RunCoroutine>c__Iterator.<>f__this = this;
            //return <RunCoroutine>c__Iterator;
            if (!this.mStop)
            {
                if (this.mCoroutineEnumerator == null || !this.mCoroutineEnumerator.MoveNext())
                {
                    yield break;
                }

            }

            mParent.TaskCoroutineEnded(this, mCoroutineName);
        }
	}
}

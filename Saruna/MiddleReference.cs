using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saruna
{
	public class MiddleReference<T> where T : class
	{
		T reference;
		WeakReference<T> weakReference;
		
		public MiddleReference(T target)
		{
			if (GC.GetGeneration(target) == 0)
				reference = target;
			else
				weakReference = new WeakReference<T>(target);
		}

		public T GetOrSetTarget(Func<T> creator)
		{
			T target;
			if (reference != null)
			{
				if (GC.GetGeneration(target = reference) > 0)
				{
					weakReference = new WeakReference<T>(reference);
					reference = null;
				}
				return target;
			}
			if (weakReference.TryGetTarget(out target))
				return target;
			if ((target = creator()) == null)
				return target;
			if (GC.GetGeneration(target) == 0)
				reference = target;
			else
				weakReference.SetTarget(target);
			return target;
		}
	}
}

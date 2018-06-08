// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Oscar.Bot
{
    public class BaseState : Dictionary<string, object>
    {
        public BaseState(IDictionary<string, object> source)
        {
            if (source != null)
            {
                source.ToList().ForEach(x => this.Add(x.Key, x.Value));
            }
        }

        protected T GetProperty<T>([CallerMemberName]string propName = null)
        {
            if (this.TryGetValue(propName, out object value))
            {
                return (T)value;
            }
            return default(T);
        }

        protected void SetProperty(object value, [CallerMemberName]string propName = null)
        {
            this[propName] = value;
        }
    }

	public class UserState : BaseState
    {
        public UserState() : base(null) { }

        public UserState(IDictionary<string, object> source) : base(source) { }

        public IList<EpisodeInquiry> EpisodeInquiries
        {
            get { return GetProperty<IList<EpisodeInquiry>>(); }
            set { SetProperty(value); }
        }
    }

	public class EpisodeInquiry : BaseState
	{
		public EpisodeInquiry() : base(null) { }

		public EpisodeInquiry(IDictionary<string, object> source = null) : base(source) { }

		public string MediaTitle
		{
			get { return GetProperty<string>(); }
			set { SetProperty(value); }
		}

		public Episode Episode
		{
			get { return GetProperty<Episode>(); }
			set { SetProperty(value); }
		}

		public Show Show
		{
			get { return GetProperty<Show>(); }
			set { SetProperty(value); }
		}
		
		public bool ProvideAdditionalInfo
		{
			get { return GetProperty<bool>(); }
			set { SetProperty(value); }
		}

	}
}

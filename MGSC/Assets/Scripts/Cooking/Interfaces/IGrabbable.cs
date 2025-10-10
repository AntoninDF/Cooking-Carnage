using System;

namespace Cooking
{

    public interface IGrabbable
    {
        public IContainer Container { get; set; }
        public Action OnGrabbed { get; set; }
        public void GetGrabbed(IContainer newContainer);
    }

}
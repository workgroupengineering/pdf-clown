using Microsoft.AspNetCore.Components;
using System.Reflection;

namespace PdfClown.UI.Blazor.Internal
{
    public class Animation
    {
        private static readonly Dictionary<ComponentBase, Dictionary<string, Animation>> cache = new();

        public static void Abort(ComponentBase component, string name)
        {
            if (cache.TryGetValue(component, out var names)
                && names.TryGetValue(name, out var animation))
            {
                animation.Cancel();
            }
        }

        private Action<double> action;
        private double currentValue;
        private double newValue;
        private Easing easing;
        private double step;
        private bool cancel;
        private Timer timer;
        private string name;
        private Dictionary<string, Animation> names;
        private Action<double, bool> finished;
        private List<Animation> animations;
        private Animation Parent;

        public Animation()
        {
        }

        public Animation(Action<double> action, double currentValue, double newValue, Easing easing)
        {
            this.action = action;
            this.currentValue = currentValue;
            this.newValue = newValue;
            this.easing = easing;
        }

        public bool IsCancel => cancel || (Parent?.IsCancel ?? false);

        internal void Add(int v1, int v2, Animation animation)
        {
            animations ??= new List<Animation>();
            animations.Add(animation);
            animation.Parent = this;
        }

        internal void Commit(ComponentBase component, string name, uint rate, uint length, Action<double, bool> finished)
        {
            if (!cache.TryGetValue(component, out var names))
                cache[component] = names = new Dictionary<string, Animation>();
            //if (names.TryGetValue(name, out var animation))
            //    throw new Exception("Dublicat");
            this.name = name;
            this.names = names;
            names[name] = this;
            cancel = false;
            if (action != null)
            {
                Run(rate, length, finished);
            }
            else
            {
                this.finished = finished;
                foreach (var item in animations)
                {
                    item.cancel = cancel;
                    item.Run(rate, length, (d, b) =>
                    {
                        if (animations.All(x => x.timer == null))
                        {
                            Finish();
                        }
                    });
                }
            }
        }

        private void Run(uint rate, uint legth, Action<double, bool> finished)
        {
            this.finished = finished;
            var expectedTicks = legth / rate;
            var diff = newValue - currentValue;
            step = diff / expectedTicks;
            if (Math.Abs(diff) < 0.001 || step == 0D || IsCancel)
            {
                action(newValue);
                Finish();
                return;
            }
            timer = new Timer(TimerTick, this, 0, rate);
        }

        public static void TimerTick(object state)
        {
            var animation = (Animation)state;
            animation.currentValue += animation.step;
            animation.action(animation.currentValue);
            if (Math.Abs(animation.currentValue - animation.newValue) < 0.001
                || animation.IsCancel)
            {
                animation.Finish();
            }
        }

        private void Finish()
        {
            timer?.Dispose();
            timer = null;
            names?.Remove(name);
            names = null;
            finished?.Invoke(currentValue, !IsCancel);            
        }

        private void Cancel()
        {
            cancel = true;
        }


    }
}
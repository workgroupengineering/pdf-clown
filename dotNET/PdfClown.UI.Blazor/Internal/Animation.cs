using Microsoft.AspNetCore.Components;
using System.Collections.Concurrent;
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

        private Action<double>? action;
        private double currentValue;
        private double newValue;
        private Easing easing;
        private double step;
        private bool cancel;
        private Timer? timer;
        private string name;
        private Dictionary<string, Animation>? names;
        private Action<double, bool>? finished;
        private List<Animation>? animations;
        private Animation? Parent;

        public Animation()
        {
            name = "Multi";
        }

        public Animation(Action<double> action, double currentValue, double newValue, Easing easing)
        {
            name = string.Empty;
            this.action = action;
            this.currentValue = currentValue;
            this.newValue = newValue;
            this.easing = easing;
        }

        public bool IsCancel => cancel || (Parent?.IsCancel ?? false);

        public bool IsIncrease => newValue > currentValue;

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
            if (names.TryGetValue(name, out var animation))
            {
                if (IsIncrease == animation.IsIncrease
                    && (IsIncrease && newValue > animation.newValue
                        || !IsIncrease && newValue < animation.newValue))
                {
                    animation.newValue = newValue;
                    animation.step = CalculateStep(rate, length);
                }
                return;
            }
            this.name = name;
            this.names = names;
            names[name] = this;
            cancel = false;
            if (action != null)
            {
                Run(rate, length, finished);
            }
            else if (animations != null)
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

        private double CalculateStep(uint rate, uint legth)
        {
            var expectedTicks = legth / rate;
            var diff = newValue - currentValue;
            return diff / expectedTicks;
        }

        private void Run(uint rate, uint legth, Action<double, bool> finished)
        {
            this.finished = finished;
            step = CalculateStep(rate, legth);
            if (Math.Abs(step) < 0.001
                || IsCancel)
            {
                action?.Invoke(newValue);
                Finish();
                return;
            }
            timer = new Timer(TimerTick, null, 0, rate);
        }

        public void TimerTick(object? sender)
        {
            currentValue += step;
            action?.Invoke(currentValue);
            if (Math.Abs(currentValue - newValue) < 0.001
                || IsCancel)
            {
                Finish();
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
            names?.Remove(name);
            names = null;
        }


    }
}
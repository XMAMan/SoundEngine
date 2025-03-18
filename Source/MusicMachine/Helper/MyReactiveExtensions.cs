using ReactiveUI;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using System.Windows.Input;

namespace MusicMachine.Helper
{
    public static class MyReactiveExtensions
    {
        public static IObservable<Unit> WhereLastKeys(this IObservable<KeyEventArgs> keysObserveable, params Key[] keys)
        {
            return keysObserveable
                .Select(x => x.Key)
                .Window(keys.Length, 1) //Erzeugt Items, welche IObserveable<Key> sind, und jeweils 5 Buchstaben enthalten
                .SelectMany(x => x.SequenceEqual(keys)) //Vergleiche Window gegen Vorgabewert und gib true/false zurück
                .Where(x => x) //Nur die true-Werte durchlassen
                .Select(x => Unit.Default); //Ein Command der keine Inputwerte hat, benötigt Unit.Default als Input
        }
    }


    /*public float Volume
        {
            get { return this.model.Synthesizer.Volume; }
            set { this.SetAndRaise(x => this.model.Synthesizer.Volume, value); }
        }*/
    public static class MyReactiveObjectExtensions
    {
        //Idee dafür: https://github.com/reactiveui/ReactiveUI/blob/master/src/ReactiveUI.Fody.Helpers/ObservableAsPropertyExtensions.cs
        public static void SetAndRaise<TObj, TRet>(this TObj source, Expression<Func<TObj, TRet>> property, TRet value) 
            where TObj : ReactiveObject
        {
            var propertyInfo = property.GetPropertyInfo();
            //var field = propertyInfo.DeclaringType.GetTypeInfo().GetDeclaredField("$" + propertyInfo.Name);
            //field.SetValue(source, value);
            var field = propertyInfo.DeclaringType.GetTypeInfo().GetDeclaredMethod("set_" + propertyInfo.Name);
            field.Invoke(source, new object[] { value });
            source.RaisePropertyChanged(propertyInfo.Name);
        }

        private static PropertyInfo GetPropertyInfo(this LambdaExpression expression)
        {
            var current = expression.Body;
            if (current is UnaryExpression unary)
            {
                current = unary.Operand;
            }

            var call = (MemberExpression)current;
            return (PropertyInfo)call.Member;
        }
    }
}

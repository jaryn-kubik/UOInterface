﻿using System;
using System.Threading.Tasks;

namespace UOInterface
{
    public static class Extensions
    {
        public static void Raise(this EventHandler handler, object sender = null)
        {
            if (handler != null)
                handler(sender, EventArgs.Empty);
        }

        public static void Raise<T>(this EventHandler<T> handler, T e, object sender = null)
        {
            if (handler != null)
                handler(sender, e);
        }

        public static void RaiseAsync(this EventHandler handler, object sender = null)
        {
            if (handler != null)
                Task.Run(() =>
                {
                    try { handler(sender, EventArgs.Empty); }
                    catch (Exception) { throw new NotImplementedException(); }
                });
        }

        public static void RaiseAsync<T>(this EventHandler<T> handler, T e, object sender = null)
        {
            if (handler != null)
                Task.Run(() =>
                {
                    try { handler(sender, e); }
                    catch (Exception) { throw new NotImplementedException(); }
                });
        }
    }
}
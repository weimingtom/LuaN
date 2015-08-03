﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LuaN.DllWrapper.Tests
{
    public class LuaStateTest
    {

        [Fact]
        public void TestCreate()
        {
            LuaState L;
            using (L = new LuaState())
            {
                Assert.NotEqual(IntPtr.Zero, L.NativeState);
                Assert.Same(LuaEngine.Current, L.Engine);

                Assert.Equal(-1001000, L.FirstPseudoIndex);
                Assert.Equal(-1, L.MultiReturns);
                Assert.Equal(-1001000, L.RegistryIndex);
                Assert.Equal(20, L.MinStack);

                Assert.Equal(503, L.LuaVersion());

            }
            Assert.Throws<ObjectDisposedException>(() => L.NativeState);
        }

        [Fact]
        public void TestLuaNewThread()
        {
            var engine = new LuaEngine();
            LuaState L, C;
            using (L = (LuaState)engine.NewState())
            {
                C = (LuaState)L.LuaNewThread();

                Assert.NotEqual(IntPtr.Zero, C.NativeState);
                Assert.NotEqual(L.NativeState, C.NativeState);
                Assert.Same(engine, C.Engine);
            }
            Assert.Throws<ObjectDisposedException>(() => L.NativeState);
            Assert.Throws<ObjectDisposedException>(() => C.NativeState);
        }

        [Fact]
        public void TestLuaAtPanic()
        {
            using (var L = new LuaState())
            {
                // Test default atpanic
                var lex = Assert.Throws<LuaException>(() => L.LuaError());
                Assert.Equal("Une exception de type 'LuaN.LuaException' a été levée.", lex.Message);
                L.LuaPushString("Test error");
                lex = Assert.Throws<LuaException>(() => L.LuaError());
                Assert.Equal("Test error", lex.Message);

                // Custom atpanic
                var oldAtPanic = L.LuaAtPanic(state =>
                {
                    throw new ApplicationException("Custom at panic");
                });
                Assert.NotNull(oldAtPanic);
                var aex = Assert.Throws<ApplicationException>(() => L.LuaError());
                Assert.Equal("Custom at panic", aex.Message);

                // Test oldPanic function
                lex = Assert.Throws<LuaException>(() => oldAtPanic(null));
                Assert.Equal("Une exception de type 'LuaN.LuaException' a été levée.", lex.Message);

                // Restore the original old panic
                Assert.True(L.RestoreOriginalAtPanic());
                Assert.False(L.RestoreOriginalAtPanic());
                L.SetDefaultAtPanic();


                Assert.Throws<ArgumentNullException>(() => L.LuaAtPanic(null));

            }
        }

        [Fact]
        public void TestWrapCFunction()
        {
            using (var L = new LuaState())
            {
                LuaDll.lua_CFunction nativeFunction = null;
                LuaCFunction cfunction = null;

                Assert.Null(L.WrapFunction(nativeFunction));
                Assert.Null(L.WrapFunction(cfunction));

                cfunction = s => 0;
                nativeFunction = L.WrapFunction(cfunction);

                Assert.Same(nativeFunction, L.WrapFunction(cfunction));
                Assert.Same(cfunction, L.WrapFunction(nativeFunction));

                nativeFunction = s => 0;
                cfunction = L.WrapFunction(nativeFunction);

                Assert.Same(nativeFunction, L.WrapFunction(cfunction));
                Assert.Same(cfunction, L.WrapFunction(nativeFunction));
            }
        }

        [Fact]
        public void TestFindInstance()
        {
            using (var L = new LuaState())
            {
                LuaState C = (LuaState)L.LuaNewThread();
                IntPtr cPtr = C.NativeState;
                C.Dispose();

                LuaCFunction cfunction = state =>
                {
                    Assert.Same(state, L);
                    return 123;
                };
                LuaDll.lua_CFunction nativeFunc = L.WrapFunction(cfunction);
                Assert.Equal(123, nativeFunc(L.NativeState));

                cfunction = state =>
                {
                    Assert.Equal(cPtr, ((LuaState)state).NativeState);
                    return 321;
                };
                nativeFunc = L.WrapFunction(cfunction);
                Assert.Equal(321, nativeFunc(cPtr));

                Assert.Equal(0, nativeFunc(IntPtr.Zero));

            }
        }

    }
}
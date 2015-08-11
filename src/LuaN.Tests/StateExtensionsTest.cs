﻿using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LuaN.Tests
{
    public class StateExtensionsTest
    {
        [Fact]
        public void TestDoFile()
        {
            var mState = new Mock<ILuaState>();
            var state = mState.Object;

            StateExtensions.DoFile(state, "test");
            mState.Verify(s => s.LuaLDoFile("test"), Times.Once());
        }

        [Fact]
        public void TestDoString()
        {
            var mState = new Mock<ILuaState>();
            var state = mState.Object;

            StateExtensions.DoString(state, "test");
            mState.Verify(s => s.LuaLDoString("test"), Times.Once());
        }

        [Fact]
        public void TestLoadFile()
        {
            var mState = new Mock<ILuaState>();
            var state = mState.Object;

            StateExtensions.LoadFile(state, "test");
            mState.Verify(s => s.LuaLLoadFile("test"), Times.Once());
        }

        [Fact]
        public void TestLoadString()
        {
            var mState = new Mock<ILuaState>();
            var state = mState.Object;

            StateExtensions.LoadString(state, "test");
            mState.Verify(s => s.LuaLLoadString("test"), Times.Once());
        }

        [Fact]
        public void TestLoadBuffer()
        {
            var mState = new Mock<ILuaState>();
            var state = mState.Object;

            var buffer = new byte[10];
            StateExtensions.LoadBuffer(state, buffer, "test");
            mState.Verify(s => s.LuaLLoadBuffer(buffer, 10, "test"), Times.Once());

            StateExtensions.LoadBuffer(state, buffer, "test", "mode");
            mState.Verify(s => s.LuaLLoadBufferX(buffer, 10, "test", "mode"), Times.Once());

            StateExtensions.LoadBuffer(state, "content", "test");
            mState.Verify(s => s.LuaLLoadBuffer(It.IsAny<byte[]>(), 7, "test"), Times.Once());

            StateExtensions.LoadBuffer(state, (string)null, "chunk");
            mState.Verify(s => s.LuaLLoadBuffer(It.IsAny<byte[]>(), 0, "chunk"), Times.Once());

            mState.ResetCalls();
            StateExtensions.LoadBuffer(state, (byte[])null, "chunk");
            mState.Verify(s => s.LuaLLoadBuffer(It.IsAny<byte[]>(), 0, "chunk"), Times.Once());

            mState.ResetCalls();
            StateExtensions.LoadBuffer(state, (byte[])null, "chunk", "mode");
            mState.Verify(s => s.LuaLLoadBufferX(It.IsAny<byte[]>(), 0, "chunk", "mode"), Times.Once());
        }

        [Fact]
        public void TestLuaRef()
        {
            var mState = new Mock<ILuaState>();
            mState.Setup(s => s.LuaLRef(It.IsAny<int>())).Returns(5);
            var state = mState.Object;

            Assert.Equal(5, state.LuaRef());
            mState.Verify(s => s.LuaLRef(state.RegistryIndex));
        }

        [Fact]
        public void TestLuaPushRef()
        {
            var mState = new Mock<ILuaState>();
            var state = mState.Object;

            state.LuaPushRef(5);
            state.LuaPushRef(state.RegistryIndex, 5);
            mState.Verify(s => s.LuaRawGetI(state.RegistryIndex, 5), Times.Exactly(2));
        }

        [Fact]
        public void TestLuaUnref()
        {
            var mState = new Mock<ILuaState>();
            var state = mState.Object;

            state.LuaUnref(5);
            mState.Verify(s => s.LuaLUnref(state.RegistryIndex, 5), Times.Exactly(1));
        }

        [Fact]
        public void TestLuaToInteger()
        {
            var mState = new Mock<ILuaState>();
            var state = mState.Object;

            bool isnum;
            state.LuaToInteger(5, out isnum);
            mState.Verify(s => s.LuaToIntegerX(5, out isnum), Times.Exactly(1));
        }

        [Fact]
        public void TestLuaToUnsigned()
        {
            var mState = new Mock<ILuaState>();
            var state = mState.Object;

            bool isnum;
            state.LuaToUnsigned(5, out isnum);
            mState.Verify(s => s.LuaToUnsignedX(5, out isnum), Times.Exactly(1));
        }

        [Fact]
        public void TestLuaToNumber()
        {
            var mState = new Mock<ILuaState>();
            var state = mState.Object;

            bool isnum;
            state.LuaToNumber(5, out isnum);
            mState.Verify(s => s.LuaToNumberX(5, out isnum), Times.Exactly(1));
        }

        [Fact]
        public void TestToUserData()
        {
            var mState = new Mock<ILuaState>();
            var state = mState.Object;

            Assert.Null(state.ToUserData<Lua>(-3));
            Assert.Equal(0, state.ToUserData<int>(-3));

            mState.Verify(s => s.LuaToUserData(-3), Times.Exactly(2));
        }

        [Fact]
        public void TestPush()
        {
            var mState = new Mock<ILuaState>();
            var state = mState.Object;

            state.Push(null);
            mState.Verify(s => s.LuaPushNil(), Times.Exactly(1));

            state.Push(true);
            mState.Verify(s => s.LuaPushBoolean(true), Times.Exactly(1));

            state.Push(123);
            mState.Verify(s => s.LuaPushInteger(123), Times.Exactly(1));

            state.Push(123.45);
            mState.Verify(s => s.LuaPushNumber(123.45), Times.Exactly(1));

            state.Push("test");
            mState.Verify(s => s.LuaPushString("test"), Times.Exactly(1));

            Assert.Throws<InvalidOperationException>(() => state.Push(new Mock<ILuaNativeUserData>().Object));

            LuaCFunction func = s => 0;
            state.Push(func);
            mState.Verify(s => s.LuaPushCFunction(func), Times.Exactly(1));

            state.Push(state);
            mState.Verify(s => s.LuaPushThread(), Times.Exactly(1));

            Assert.Throws<InvalidOperationException>(() => state.Push(new Mock<ILuaState>().Object));

            state.Push(this);
            mState.Verify(s => s.LuaPushLightUserData(this), Times.Exactly(1));

        }

        [Fact]
        public void TestToObject()
        {
            var mState = new Mock<ILuaState>();
            mState.Setup(s => s.LuaType(1)).Returns(LuaType.Nil);
            mState.Setup(s => s.LuaType(2)).Returns(LuaType.Boolean);
            mState.Setup(s => s.LuaType(3)).Returns(LuaType.Number);
            mState.Setup(s => s.LuaType(4)).Returns(LuaType.String);
            mState.Setup(s => s.LuaType(5)).Returns(LuaType.LightUserData);
            mState.Setup(s => s.LuaType(6)).Returns(LuaType.UserData);
            mState.Setup(s => s.LuaType(7)).Returns(LuaType.Table);
            mState.Setup(s => s.LuaType(8)).Returns(LuaType.Function);
            mState.Setup(s => s.LuaType(9)).Returns(LuaType.Thread);
            var state = mState.Object;

            Assert.Equal(null, state.ToObject(1));

            Assert.Equal(false, state.ToObject(2));
            mState.Verify(s => s.LuaToBoolean(2), Times.Once());

            Assert.Equal(0d, state.ToObject(3));
            mState.Verify(s => s.LuaToNumber(3), Times.Once());

            Assert.Equal(null, state.ToObject(4));
            mState.Verify(s => s.LuaToString(4), Times.Once());

            Assert.Equal(null, state.ToObject(5));
            mState.Verify(s => s.LuaToUserData(5), Times.Once());

            Assert.Equal(null, state.ToObject(6));
            mState.Verify(s => s.LuaToUserData(6), Times.Once());

            Assert.Throws<NotImplementedException>(() => Assert.Equal(null, state.ToObject(7)));
            //mState.Verify(s => s.LuaToUserData(7), Times.Once());

            Assert.Throws<NotImplementedException>(() => Assert.Equal(null, state.ToObject(8)));
            //mState.Verify(s => s.LuaToUserData(8), Times.Once());

            Assert.Equal(null, state.ToObject(9));
            mState.Verify(s => s.LuaToThread(9), Times.Once());

        }

    }
}

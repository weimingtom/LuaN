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

            StateExtensions.LuaDoFile(state, "test");
            mState.Verify(s => s.LuaLDoFile("test"), Times.Once());
        }

        [Fact]
        public void TestDoString()
        {
            var mState = new Mock<ILuaState>();
            var state = mState.Object;

            StateExtensions.LuaDoString(state, "test");
            mState.Verify(s => s.LuaLDoString("test"), Times.Once());
        }

        [Fact]
        public void TestLoadFile()
        {
            var mState = new Mock<ILuaState>();
            var state = mState.Object;

            StateExtensions.LuaLoadFile(state, "test");
            mState.Verify(s => s.LuaLLoadFile("test"), Times.Once());
        }

        [Fact]
        public void TestLoadString()
        {
            var mState = new Mock<ILuaState>();
            var state = mState.Object;

            StateExtensions.LuaLoadString(state, "test");
            mState.Verify(s => s.LuaLLoadString("test"), Times.Once());
        }

        [Fact]
        public void TestLoadBuffer()
        {
            var mState = new Mock<ILuaState>();
            var state = mState.Object;

            var buffer = new byte[10];
            StateExtensions.LuaLoadBuffer(state, buffer, "test");
            mState.Verify(s => s.LuaLLoadBuffer(buffer, 10, "test"), Times.Once());

            StateExtensions.LuaLoadBuffer(state, buffer, "test", "mode");
            mState.Verify(s => s.LuaLLoadBufferX(buffer, 10, "test", "mode"), Times.Once());

            StateExtensions.LuaLoadBuffer(state, "content", "test");
            mState.Verify(s => s.LuaLLoadBuffer(It.IsAny<byte[]>(), 7, "test"), Times.Once());

            StateExtensions.LuaLoadBuffer(state, (string)null, "chunk");
            mState.Verify(s => s.LuaLLoadBuffer(It.IsAny<byte[]>(), 0, "chunk"), Times.Once());

            mState.ResetCalls();
            StateExtensions.LuaLoadBuffer(state, (byte[])null, "chunk");
            mState.Verify(s => s.LuaLLoadBuffer(It.IsAny<byte[]>(), 0, "chunk"), Times.Once());

            mState.ResetCalls();
            StateExtensions.LuaLoadBuffer(state, (byte[])null, "chunk", "mode");
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
        public void TestLuaToUserData()
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

            var mLuaValue = new Mock<ILuaValue>();
            state.Push(mLuaValue.Object);
            mLuaValue.Verify(v => v.Push(state), Times.Once());

            mState = new Mock<ILuaState>();
            var mDotnet = mState.As<ILuaDotnet>();
            mState.Setup(s => s.GetService(typeof(ILuaDotnet))).Returns(mDotnet.Object);
            state = mState.Object;
            state.Push(this);
            mDotnet.Verify(s => s.Push(this), Times.Exactly(1));
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
            mState.Setup(s => s.LuaIsUserData(6)).Returns(true);
            mState.Setup(s => s.LuaType(7)).Returns(LuaType.Table);
            mState.Setup(s => s.LuaType(8)).Returns(LuaType.Function);
            mState.Setup(s => s.LuaType(9)).Returns(LuaType.Thread);
            var state = mState.Object;

            Assert.Equal(null, StateExtensions.ToObject(null, 2));
            Assert.Equal(null, state.ToObject(1));

            Assert.Equal(false, state.ToObject(2));
            mState.Verify(s => s.LuaToBoolean(2), Times.Once());

            Assert.Equal(0d, state.ToObject(3));
            mState.Verify(s => s.LuaToNumber(3), Times.Once());

            Assert.Equal(null, state.ToObject(4));
            mState.Verify(s => s.LuaToString(4), Times.Once());

            Assert.Equal(null, state.ToObject(5));
            mState.Verify(s => s.LuaToUserData(5), Times.Once());

            Assert.IsAssignableFrom<ILuaUserData>(state.ToObject(6));
            //mState.Verify(s => s.LuaToUserData(6), Times.Once());

            Assert.IsAssignableFrom<ILuaTable>(state.ToObject(7));
            //mState.Verify(s => s.LuaToUserData(7), Times.Once());

            Assert.Equal(null, state.ToObject(8));
            //mState.Verify(s => s.LuaToUserData(8), Times.Once());

            Assert.Equal(null, state.ToObject(9));
            mState.Verify(s => s.LuaToThread(9), Times.Once());

            mState = new Mock<ILuaState>();
            var mDotnet = mState.As<ILuaDotnet>();
            mState.Setup(s => s.GetService(typeof(ILuaDotnet))).Returns(mDotnet.Object);
            state = mState.Object;
            state.ToValue(123);
            mDotnet.Verify(s => s.ToValue(123), Times.Exactly(1));

        }

        [Fact]
        public void TestCallFunction()
        {
            LuaCFunction func = s => 0;
            // No results
            int top = 0;
            var mState = new Mock<ILuaState>();
            mState.SetupGet(s => s.MultiReturns).Returns(-1);
            mState.Setup(s => s.LuaGetTop()).Returns(() => top);
            mState.Setup(s => s.LuaPushCFunction(func)).Callback(() => { top++; });
            mState.Setup(s => s.LuaPCall(3, -1, 0)).Callback(() => { top = 0; });
            using (var l = mState.Object)
            {
                var result = l.CallFunction(func, new Object[] { "field1", null, 12.34 });
                Assert.Equal(new Object[0], result);
                mState.Verify(s => s.LuaPushCFunction(func), Times.Once());
                mState.Verify(s => s.LuaPushString("field1"), Times.Once());
                mState.Verify(s => s.LuaPushNil(), Times.Once());
                mState.Verify(s => s.LuaPushNumber(12.34), Times.Once());
                mState.Verify(s => s.LuaPCall(3, -1, 0), Times.Once());
            }

            // Call failed
            top = 0;
            mState = new Mock<ILuaState>();
            mState.SetupGet(s => s.MultiReturns).Returns(-1);
            mState.Setup(s => s.LuaGetTop()).Returns(() => top);
            mState.Setup(s => s.LuaPushCFunction(func)).Callback(() => { top++; });
            mState.Setup(s => s.LuaPCall(3, -1, 0)).Returns(() => { top = 1; return LuaStatus.ErrorRun; });
            mState.Setup(_ => _.LuaType(-1)).Returns(LuaType.Nil);
            using (var l = mState.Object)
            {
                var ex = Assert.Throws<LuaException>(() => l.CallFunction(func, new Object[] { "field1", null, 12.34 }));
                Assert.Equal("Unknown Lua error.", ex.Message);
            }
            // Call failed
            top = 0;
            mState = new Mock<ILuaState>();
            mState.SetupGet(s => s.MultiReturns).Returns(-1);
            mState.Setup(s => s.LuaGetTop()).Returns(() => top);
            mState.Setup(s => s.LuaPushCFunction(func)).Callback(() => { top++; });
            mState.Setup(s => s.LuaPCall(3, -1, 0)).Returns(() => { top = 1; return LuaStatus.ErrorRun; });
            mState.Setup(_ => _.LuaType(-1)).Returns(LuaType.String);
            mState.Setup(_ => _.LuaToString(-1)).Returns("Error in the call");
            using (var l = mState.Object)
            {
                var ex = Assert.Throws<LuaException>(() => l.CallFunction(func, new Object[] { "field1", null, 12.34 }));
                Assert.Equal("Error in the call", ex.Message);
            }

            // Multiple results
            top = 0;
            mState = new Mock<ILuaState>();
            mState.SetupGet(s => s.MultiReturns).Returns(-1);
            mState.Setup(s => s.LuaGetTop()).Returns(() => top);
            mState.Setup(s => s.LuaPushCFunction(func)).Callback(() => { top++; });
            mState.Setup(s => s.LuaPCall(3, -1, 0)).Callback(() => { top = 4; });
            mState.Setup(_ => _.LuaType(1)).Returns(LuaType.Nil);
            mState.Setup(_ => _.LuaType(2)).Returns(LuaType.Boolean);
            mState.Setup(_ => _.LuaToBoolean(2)).Returns(true);
            mState.Setup(_ => _.LuaType(3)).Returns(LuaType.Number);
            mState.Setup(_ => _.LuaToNumber(3)).Returns(123.45);
            mState.Setup(_ => _.LuaType(4)).Returns(LuaType.String);
            mState.Setup(_ => _.LuaToString(4)).Returns("Test");
            using (var l = mState.Object)
            {
                var result = l.CallFunction(func, new Object[] { "field1", null, 12.34 });
                Assert.Equal(new Object[] { null, true, 123.45d, "Test" }, result);
                mState.Verify(s => s.LuaPushCFunction(func), Times.Once());
                mState.Verify(s => s.LuaPushString("field1"), Times.Once());
                mState.Verify(s => s.LuaPushNil(), Times.Once());
                mState.Verify(s => s.LuaPushNumber(12.34), Times.Once());
                mState.Verify(s => s.LuaPCall(3, -1, 0), Times.Once());
            }

            // Multiple results typed
            top = 0;
            mState = new Mock<ILuaState>();
            mState.SetupGet(s => s.MultiReturns).Returns(-1);
            mState.Setup(s => s.LuaGetTop()).Returns(() => top);
            mState.Setup(s => s.LuaPushCFunction(func)).Callback(() => { top++; });
            mState.Setup(s => s.LuaPCall(3, -1, 0)).Callback(() => { top = 4; });
            mState.Setup(_ => _.LuaType(1)).Returns(LuaType.Nil);
            mState.Setup(_ => _.LuaType(2)).Returns(LuaType.Boolean);
            mState.Setup(_ => _.LuaToBoolean(2)).Returns(true);
            mState.Setup(_ => _.LuaType(3)).Returns(LuaType.Number);
            mState.Setup(_ => _.LuaToNumber(3)).Returns(123.45);
            mState.Setup(_ => _.LuaType(4)).Returns(LuaType.String);
            mState.Setup(_ => _.LuaToString(4)).Returns("Test");
            using (var l = mState.Object)
            {
                var result = l.CallFunction(func, new Object[] { "field1", null, 12.34 }, new Type[] { typeof(Lua), typeof(int) });
                Assert.Equal(new Object[] { null, 1 }, result);
                mState.Verify(s => s.LuaPushCFunction(func), Times.Once());
                mState.Verify(s => s.LuaPushString("field1"), Times.Once());
                mState.Verify(s => s.LuaPushNil(), Times.Once());
                mState.Verify(s => s.LuaPushNumber(12.34), Times.Once());
                mState.Verify(s => s.LuaPCall(3, -1, 0), Times.Once());
            }

            // Multiple results typed
            top = 0;
            mState = new Mock<ILuaState>();
            mState.SetupGet(s => s.MultiReturns).Returns(-1);
            mState.Setup(s => s.LuaGetTop()).Returns(() => top);
            mState.Setup(s => s.LuaPushCFunction(func)).Callback(() => { top++; });
            mState.Setup(s => s.LuaPCall(3, -1, 0)).Callback(() => { top = 4; });
            mState.Setup(_ => _.LuaType(1)).Returns(LuaType.Nil);
            mState.Setup(_ => _.LuaType(2)).Returns(LuaType.Boolean);
            mState.Setup(_ => _.LuaToBoolean(2)).Returns(true);
            mState.Setup(_ => _.LuaType(3)).Returns(LuaType.Number);
            mState.Setup(_ => _.LuaToNumber(3)).Returns(123.45);
            mState.Setup(_ => _.LuaType(4)).Returns(LuaType.String);
            mState.Setup(_ => _.LuaToString(4)).Returns("Test");
            using (var l = mState.Object)
            {
                var result = l.CallFunction(func, new Object[] { "field1", null, 12.34 }, new Type[] { typeof(Lua), typeof(Lua), typeof(int), null });
                Assert.Equal(new Object[] { null, null, 123, "Test" }, result);
                mState.Verify(s => s.LuaPushCFunction(func), Times.Once());
                mState.Verify(s => s.LuaPushString("field1"), Times.Once());
                mState.Verify(s => s.LuaPushNil(), Times.Once());
                mState.Verify(s => s.LuaPushNumber(12.34), Times.Once());
                mState.Verify(s => s.LuaPCall(3, -1, 0), Times.Once());
            }

            // Multiple results typed
            top = 0;
            mState = new Mock<ILuaState>();
            mState.SetupGet(s => s.MultiReturns).Returns(-1);
            mState.Setup(s => s.LuaGetTop()).Returns(() => top);
            mState.Setup(s => s.LuaPushCFunction(func)).Callback(() => { top++; });
            mState.Setup(s => s.LuaPCall(3, -1, 0)).Callback(() => { top = 4; });
            mState.Setup(_ => _.LuaType(1)).Returns(LuaType.Nil);
            mState.Setup(_ => _.LuaType(2)).Returns(LuaType.Boolean);
            mState.Setup(_ => _.LuaToBoolean(2)).Returns(true);
            mState.Setup(_ => _.LuaType(3)).Returns(LuaType.Number);
            mState.Setup(_ => _.LuaToNumber(3)).Returns(123.45);
            mState.Setup(_ => _.LuaType(4)).Returns(LuaType.String);
            mState.Setup(_ => _.LuaToString(4)).Returns("Test");
            using (var l = mState.Object)
            {
                var result = l.CallFunction(func, new Object[] { "field1", null, 12.34 }, new Type[] { typeof(Lua), typeof(Lua), typeof(int), typeof(DateTime), typeof(double) });
                Assert.Equal(new Object[] { null, null, 123, DateTime.MinValue, 0d }, result);
                mState.Verify(s => s.LuaPushCFunction(func), Times.Once());
                mState.Verify(s => s.LuaPushString("field1"), Times.Once());
                mState.Verify(s => s.LuaPushNil(), Times.Once());
                mState.Verify(s => s.LuaPushNumber(12.34), Times.Once());
                mState.Verify(s => s.LuaPCall(3, -1, 0), Times.Once());
            }
        }

        [Fact]
        public void TestCallValue()
        {
            // No results
            int top = 0;
            var mState = new Mock<ILuaState>();
            mState.SetupGet(s => s.MultiReturns).Returns(-1);
            mState.Setup(s => s.LuaGetTop()).Returns(() => top);
            mState.Setup(s => s.LuaRawGetI(It.IsAny<int>(), 123)).Callback(() => { top++; });
            mState.Setup(s => s.LuaPCall(3, -1, 0)).Callback(() => { top = 0; });
            using (var l = mState.Object)
            {
                var result = l.CallValue(123, new Object[] { "field1", null, 12.34 });
                Assert.Equal(new Object[0], result);
                mState.Verify(s => s.LuaRawGetI(l.RegistryIndex, 123), Times.Once());
                mState.Verify(s => s.LuaPushString("field1"), Times.Once());
                mState.Verify(s => s.LuaPushNil(), Times.Once());
                mState.Verify(s => s.LuaPushNumber(12.34), Times.Once());
                mState.Verify(s => s.LuaPCall(3, -1, 0), Times.Once());
            }

            // Call failed
            top = 0;
            mState = new Mock<ILuaState>();
            mState.SetupGet(s => s.MultiReturns).Returns(-1);
            mState.Setup(s => s.LuaGetTop()).Returns(() => top);
            mState.Setup(s => s.LuaRawGetI(It.IsAny<int>(), 123)).Callback(() => { top++; });
            mState.Setup(s => s.LuaPCall(3, -1, 0)).Returns(() => { top = 1; return LuaStatus.ErrorRun; });
            mState.Setup(_ => _.LuaType(-1)).Returns(LuaType.Nil);
            using (var l = mState.Object)
            {
                var ex = Assert.Throws<LuaException>(() => l.CallValue(123, new Object[] { "field1", null, 12.34 }));
                Assert.Equal("Unknown Lua error.", ex.Message);
            }
            // Call failed
            top = 0;
            mState = new Mock<ILuaState>();
            mState.SetupGet(s => s.MultiReturns).Returns(-1);
            mState.Setup(s => s.LuaGetTop()).Returns(() => top);
            mState.Setup(s => s.LuaRawGetI(It.IsAny<int>(), 123)).Callback(() => { top++; });
            mState.Setup(s => s.LuaPCall(3, -1, 0)).Returns(() => { top = 1; return LuaStatus.ErrorRun; });
            mState.Setup(_ => _.LuaType(-1)).Returns(LuaType.String);
            mState.Setup(_ => _.LuaToString(-1)).Returns("Error in the call");
            using (var l = mState.Object)
            {
                var ex = Assert.Throws<LuaException>(() => l.CallValue(123, new Object[] { "field1", null, 12.34 }));
                Assert.Equal("Error in the call", ex.Message);
            }

            // Multiple results
            top = 0;
            mState = new Mock<ILuaState>();
            mState.SetupGet(s => s.MultiReturns).Returns(-1);
            mState.Setup(s => s.LuaGetTop()).Returns(() => top);
            mState.Setup(s => s.LuaRawGetI(It.IsAny<int>(), 123)).Callback(() => { top++; });
            mState.Setup(s => s.LuaPCall(3, -1, 0)).Callback(() => { top = 4; });
            mState.Setup(_ => _.LuaType(1)).Returns(LuaType.Nil);
            mState.Setup(_ => _.LuaType(2)).Returns(LuaType.Boolean);
            mState.Setup(_ => _.LuaToBoolean(2)).Returns(true);
            mState.Setup(_ => _.LuaType(3)).Returns(LuaType.Number);
            mState.Setup(_ => _.LuaToNumber(3)).Returns(123.45);
            mState.Setup(_ => _.LuaType(4)).Returns(LuaType.String);
            mState.Setup(_ => _.LuaToString(4)).Returns("Test");
            using (var l = mState.Object)
            {
                var result = l.CallValue(123, new Object[] { "field1", null, 12.34 });
                Assert.Equal(new Object[] { null, true, 123.45d, "Test" }, result);
                mState.Verify(s => s.LuaRawGetI(l.RegistryIndex, 123), Times.Once());
                mState.Verify(s => s.LuaPushString("field1"), Times.Once());
                mState.Verify(s => s.LuaPushNil(), Times.Once());
                mState.Verify(s => s.LuaPushNumber(12.34), Times.Once());
                mState.Verify(s => s.LuaPCall(3, -1, 0), Times.Once());
            }

            // Multiple results typed
            top = 0;
            mState = new Mock<ILuaState>();
            mState.SetupGet(s => s.MultiReturns).Returns(-1);
            mState.Setup(s => s.LuaGetTop()).Returns(() => top);
            mState.Setup(s => s.LuaRawGetI(It.IsAny<int>(), 123)).Callback(() => { top++; });
            mState.Setup(s => s.LuaPCall(3, -1, 0)).Callback(() => { top = 4; });
            mState.Setup(_ => _.LuaType(1)).Returns(LuaType.Nil);
            mState.Setup(_ => _.LuaType(2)).Returns(LuaType.Boolean);
            mState.Setup(_ => _.LuaToBoolean(2)).Returns(true);
            mState.Setup(_ => _.LuaType(3)).Returns(LuaType.Number);
            mState.Setup(_ => _.LuaToNumber(3)).Returns(123.45);
            mState.Setup(_ => _.LuaType(4)).Returns(LuaType.String);
            mState.Setup(_ => _.LuaToString(4)).Returns("Test");
            using (var l = mState.Object)
            {
                var result = l.CallValue(123, new Object[] { "field1", null, 12.34 }, new Type[] { typeof(Lua), typeof(int) });
                Assert.Equal(new Object[] { null, 1 }, result);
                mState.Verify(s => s.LuaRawGetI(l.RegistryIndex, 123), Times.Once());
                mState.Verify(s => s.LuaPushString("field1"), Times.Once());
                mState.Verify(s => s.LuaPushNil(), Times.Once());
                mState.Verify(s => s.LuaPushNumber(12.34), Times.Once());
                mState.Verify(s => s.LuaPCall(3, -1, 0), Times.Once());
            }

            // Multiple results typed
            top = 0;
            mState = new Mock<ILuaState>();
            mState.SetupGet(s => s.MultiReturns).Returns(-1);
            mState.Setup(s => s.LuaGetTop()).Returns(() => top);
            mState.Setup(s => s.LuaRawGetI(It.IsAny<int>(), 123)).Callback(() => { top++; });
            mState.Setup(s => s.LuaPCall(3, -1, 0)).Callback(() => { top = 4; });
            mState.Setup(_ => _.LuaType(1)).Returns(LuaType.Nil);
            mState.Setup(_ => _.LuaType(2)).Returns(LuaType.Boolean);
            mState.Setup(_ => _.LuaToBoolean(2)).Returns(true);
            mState.Setup(_ => _.LuaType(3)).Returns(LuaType.Number);
            mState.Setup(_ => _.LuaToNumber(3)).Returns(123.45);
            mState.Setup(_ => _.LuaType(4)).Returns(LuaType.String);
            mState.Setup(_ => _.LuaToString(4)).Returns("Test");
            using (var l = mState.Object)
            {
                var result = l.CallValue(123, new Object[] { "field1", null, 12.34 }, new Type[] { typeof(Lua), typeof(Lua), typeof(int), null });
                Assert.Equal(new Object[] { null, null, 123, "Test" }, result);
                mState.Verify(s => s.LuaRawGetI(l.RegistryIndex, 123), Times.Once());
                mState.Verify(s => s.LuaPushString("field1"), Times.Once());
                mState.Verify(s => s.LuaPushNil(), Times.Once());
                mState.Verify(s => s.LuaPushNumber(12.34), Times.Once());
                mState.Verify(s => s.LuaPCall(3, -1, 0), Times.Once());
            }

            // Multiple results typed
            top = 0;
            mState = new Mock<ILuaState>();
            mState.SetupGet(s => s.MultiReturns).Returns(-1);
            mState.Setup(s => s.LuaGetTop()).Returns(() => top);
            mState.Setup(s => s.LuaRawGetI(It.IsAny<int>(), 123)).Callback(() => { top++; });
            mState.Setup(s => s.LuaPCall(3, -1, 0)).Callback(() => { top = 4; });
            mState.Setup(_ => _.LuaType(1)).Returns(LuaType.Nil);
            mState.Setup(_ => _.LuaType(2)).Returns(LuaType.Boolean);
            mState.Setup(_ => _.LuaToBoolean(2)).Returns(true);
            mState.Setup(_ => _.LuaType(3)).Returns(LuaType.Number);
            mState.Setup(_ => _.LuaToNumber(3)).Returns(123.45);
            mState.Setup(_ => _.LuaType(4)).Returns(LuaType.String);
            mState.Setup(_ => _.LuaToString(4)).Returns("Test");
            using (var l = mState.Object)
            {
                var result = l.CallValue(123, new Object[] { "field1", null, 12.34 }, new Type[] { typeof(Lua), typeof(Lua), typeof(int), typeof(DateTime), typeof(double) });
                Assert.Equal(new Object[] { null, null, 123, DateTime.MinValue, 0d }, result);
                mState.Verify(s => s.LuaRawGetI(l.RegistryIndex, 123), Times.Once());
                mState.Verify(s => s.LuaPushString("field1"), Times.Once());
                mState.Verify(s => s.LuaPushNil(), Times.Once());
                mState.Verify(s => s.LuaPushNumber(12.34), Times.Once());
                mState.Verify(s => s.LuaPCall(3, -1, 0), Times.Once());
            }
        }

        [Fact]
        public void TestToTable()
        {
            var mState = new Mock<ILuaState>();
            mState.Setup(s => s.LuaType(1)).Returns(LuaType.Table);
            mState.Setup(s => s.LuaType(2)).Returns(LuaType.String);
            using (var state = mState.Object)
            {
                // Existing table
                using (var table = state.ToTable(1))
                {
                    Assert.IsType<LuaTable>(table);
                    var tb = table as LuaTable;
                    Assert.Same(state, tb.State);
                }
                // Not a table
                Assert.Null(state.ToTable(2));
                Assert.Null(StateExtensions.ToTable(null, 1));
            }

            ILuaTable cTable = new Mock<ILuaTable>().Object;
            mState = new Mock<ILuaState>();
            mState.Setup(s => s.LuaType(1)).Returns(LuaType.Table);
            mState.Setup(s => s.LuaType(2)).Returns(LuaType.String);
            var mDotNet = mState.As<ILuaDotnet>();
            mDotNet.Setup(s => s.ToTable(3)).Returns(cTable);
            mState.Setup(s => s.GetService(typeof(ILuaDotnet))).Returns(mDotNet.Object);
            using (var state = mState.Object)
            {
                // Existing table
                using (var table = state.ToTable(3))
                {
                    Assert.Same(cTable, table);
                }
                // Not a table
                Assert.Null(state.ToTable(2));
                Assert.Null(StateExtensions.ToTable(null, 1));
            }

            // Invalid ref
            //mState = new Mock<ILuaState>();
            //mState.Setup(s => s.LuaType(1)).Returns(LuaType.Table);
            //mState.Setup(s => s.LuaLRef(It.IsAny<int>())).Returns(LuaRef.RefNil);
            //state = mState.Object;
            //using (l = new Lua(state))
            //{
            //    var ioex = Assert.Throws<InvalidOperationException>(() => l.ToTable(1));
            //    Assert.Equal("Can't create a reference for this value.", ioex.Message);
            //}
        }

        [Fact]
        public void TestToUserData()
        {
            var mState = new Mock<ILuaState>();
            mState.Setup(s => s.LuaType(1)).Returns(LuaType.UserData);
            mState.Setup(s => s.LuaIsUserData(1)).Returns(true);
            mState.Setup(s => s.LuaType(2)).Returns(LuaType.String);
            mState.Setup(s => s.LuaIsUserData(2)).Returns(false);
            using (var state = mState.Object)
            {
                // Existing userdata
                using (var userdata = state.ToUserData(1))
                {
                    Assert.IsType<LuaUserData>(userdata);
                    var ud = userdata as LuaUserData;
                    Assert.Same(state, ud.State);
                }
                // Not an userdata
                Assert.Null(state.ToUserData(2));
                Assert.Null(StateExtensions.ToUserData(null, 2));
            }

            var cUserData = new Mock<ILuaUserData>().Object;
            mState = new Mock<ILuaState>();
            mState.Setup(s => s.LuaType(1)).Returns(LuaType.UserData);
            mState.Setup(s => s.LuaIsUserData(1)).Returns(true);
            mState.Setup(s => s.LuaType(2)).Returns(LuaType.String);
            mState.Setup(s => s.LuaIsUserData(2)).Returns(false);
            var mDotNet = mState.As<ILuaDotnet>();
            mDotNet.Setup(s => s.ToUserData(3)).Returns(cUserData);
            mState.Setup(s => s.GetService(typeof(ILuaDotnet))).Returns(mDotNet.Object);
            using (var state = mState.Object)
            {
                // Existing userdata
                using (var userdata = state.ToUserData(3))
                {
                    Assert.Same(cUserData, userdata);
                }
                // Not an userdata
                Assert.Null(state.ToUserData(2));
                Assert.Null(StateExtensions.ToUserData(null, 2));
            }

            // Invalid ref
            //mState = new Mock<ILuaState>();
            //mState.Setup(s => s.LuaType(1)).Returns(LuaType.UserData);
            //mState.Setup(s => s.LuaIsUserData(1)).Returns(true);
            //mState.Setup(s => s.LuaLRef(It.IsAny<int>())).Returns(LuaRef.RefNil);
            //state = mState.Object;
            //using (l = new Lua(state))
            //{
            //    var ioex = Assert.Throws<InvalidOperationException>(() => l.ToUserData(1));
            //    Assert.Equal("Can't create a reference for this value.", ioex.Message);
            //}
        }

        [Fact]
        public void TestToFunction()
        {
            LuaCFunction func = s => 0;

            var mState = new Mock<ILuaState>();
            mState.Setup(s => s.LuaType(1)).Returns(LuaType.Function);
            mState.Setup(s => s.LuaIsFunction(1)).Returns(true);
            mState.Setup(s => s.LuaIsCFunction(1)).Returns(true);
            mState.Setup(s => s.LuaToCFunction(1)).Returns(func);
            mState.Setup(s => s.LuaType(2)).Returns(LuaType.Function);
            mState.Setup(s => s.LuaIsFunction(2)).Returns(true);
            mState.Setup(s => s.LuaIsCFunction(2)).Returns(false);
            mState.Setup(s => s.LuaType(3)).Returns(LuaType.String);
            mState.Setup(s => s.LuaIsFunction(3)).Returns(false);
            mState.Setup(s => s.LuaIsCFunction(3)).Returns(false);
            using (var state = mState.Object)
            {
                // Existing c function
                using (var function = state.ToFunction(1))
                {
                    Assert.IsType<LuaFunction>(function);
                    var fn = function as LuaFunction;
                    Assert.Same(state, fn.State);
                    Assert.Same(func, fn.Function);
                }
                // Existing lua function
                using (var function = state.ToFunction(2))
                {
                    Assert.IsType<LuaFunction>(function);
                    var fn = function as LuaFunction;
                    Assert.Same(state, fn.State);
                }
                // Not an LuaFunction
                Assert.Null(state.ToFunction(3));
                Assert.Null(StateExtensions.ToFunction(null, 3));
            }

            var cFunction = new Mock<ILuaFunction>().Object;
            mState = new Mock<ILuaState>();
            mState.Setup(s => s.LuaType(1)).Returns(LuaType.Function);
            mState.Setup(s => s.LuaIsFunction(1)).Returns(true);
            mState.Setup(s => s.LuaIsCFunction(1)).Returns(true);
            mState.Setup(s => s.LuaToCFunction(1)).Returns(func);
            mState.Setup(s => s.LuaType(2)).Returns(LuaType.Function);
            mState.Setup(s => s.LuaIsFunction(2)).Returns(true);
            mState.Setup(s => s.LuaIsCFunction(2)).Returns(false);
            mState.Setup(s => s.LuaType(3)).Returns(LuaType.String);
            mState.Setup(s => s.LuaIsFunction(3)).Returns(false);
            mState.Setup(s => s.LuaIsCFunction(3)).Returns(false);
            var mDotNet = mState.As<ILuaDotnet>();
            mDotNet.Setup(s => s.ToFunction(4)).Returns(cFunction);
            mState.Setup(s => s.GetService(typeof(ILuaDotnet))).Returns(mDotNet.Object);
            using (var state = mState.Object)
            {
                // Existing c function
                using (var function = state.ToFunction(4))
                {
                    Assert.Same(cFunction, function);
                }
                // Existing lua function
                using (var function = state.ToFunction(4))
                {
                    Assert.Same(cFunction, function);
                }
            }

            // Invalid ref
            //mState = new Mock<ILuaState>();
            //mState.Setup(s => s.LuaType(1)).Returns(LuaType.Function);
            //mState.Setup(s => s.LuaIsFunction(1)).Returns(true);
            //mState.Setup(s => s.LuaIsCFunction(1)).Returns(false);
            //mState.Setup(s => s.LuaLRef(It.IsAny<int>())).Returns(LuaRef.RefNil);
            //state = mState.Object;
            //using (l = new Lua(state))
            //{
            //    var ioex = Assert.Throws<InvalidOperationException>(() => l.ToFunction(1));
            //    Assert.Equal("Can't create a reference for this value.", ioex.Message);
            //}
        }


    }
}
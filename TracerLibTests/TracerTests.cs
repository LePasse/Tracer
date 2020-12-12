using NUnit.Framework;
using NLog;
using System;
using System.Dynamic;
using System.Threading;
using TracerLib;

namespace Tests
{

    static public class TestEZ
    {
        static public void Test(Tracer tracer)
        {
            tracer.StartTrace();
            Thread.Sleep(10);
            tracer.StopTrace();
        }
    }
    static public class RecursionTest
    {
        static public void Test(int i, Tracer tracer)
        {
            tracer.StartTrace();
            i++;
            Thread.Sleep(10);
            if (i < 2)
            {
                Test(i, tracer);
            }
            tracer.StopTrace();
        }
    }

    static public class InsertTest
    {
        static public void InsertTestMethod(Tracer tracer)
        {
            tracer.StartTrace();
            TestEZ.Test(tracer);
            Thread.Sleep(10);
            tracer.StopTrace();
        }
    }
    static public class InsertRecursionTest
    {
        static public void Test(int i, Tracer tracer)
        {
            tracer.StartTrace();
            i++;
            TestEZ.Test(tracer);
            if (i < 2)
            {
                Test(i, tracer);
            }
            tracer.StopTrace();
        }
    }

    static public class Exc
    {
        static public void Test(Tracer tracer)
        {
            tracer.StartTrace();
            tracer = null;
            TestEZ.Test(tracer);
            tracer.StopTrace();
        }
    }
    //static public class 
    static public class Comb
    {
        static public void Test(Tracer tracer)
        {
            tracer.StartTrace();
            TestEZ.Test(tracer);
            RecursionTest.Test(1,tracer);
            tracer.StopTrace();
        }
    }

    static public class MutualRecursionTest
    {
        static public void MutualCall(int count, Tracer tracer)
        {
            tracer.StartTrace();

            if (count != 0)
                MutualCall2(count, tracer);
            Thread.Sleep(10);

            tracer.StopTrace();
        }

        static public void MutualCall2(int count, Tracer tracer)
        {
            tracer.StartTrace();

            if (count != 0)
                MutualCall(count - 1, tracer);
            Thread.Sleep(10);

            tracer.StopTrace();
        }
    }

    static public class Except
    {
        static public void Test(Tracer tracer)
        {
            tracer.StartTrace();
            RecursionTest.Test(10, tracer);
            tracer.StopTrace();
        }
    }
    public class TracerTests
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public static Tracer tracer;
        [SetUp]
        public void Setup()
        {
            tracer = new Tracer();

            var config = new NLog.Config.LoggingConfiguration();

            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "logs.txt" };

            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);

            LogManager.Configuration = config;
        }

        static void CheckAreEqual(MethodTraceResult expected, MethodTraceResult actual)
        {
            Assert.AreEqual(expected.MethodClassName, actual.MethodClassName);
            Assert.AreEqual(expected.MethodName, actual.MethodName);
            Assert.AreEqual(expected.Methods.Count, actual.Methods.Count);
            Assert.IsNotNull(actual.MethodExecuteTime);
        }

        [Test]
        public void TestEZMethod()
        {
            TestEZ.Test(tracer);
            var actual = tracer.GetTraceResult().Theards[Thread.CurrentThread.ManagedThreadId].Methods[0];
            var expected = new MethodTraceResult();
            expected.MethodName = "Test";
            expected.MethodClassName = "TestEZ";
            try
            {
                Assert.AreEqual(expected.MethodClassName, actual.MethodClassName);
                Assert.AreEqual(expected.MethodName, actual.MethodName);
                Assert.AreEqual(expected.Methods.Count, actual.Methods.Count);
                Assert.IsNotNull(actual.MethodExecuteTime);
                logger.Info(nameof(TestEZMethod) + " - passed");
            }
            catch (Exception e)
            {
                logger.Error(e, nameof(TestEZMethod) + " - failed");
            }
        }
        [Test]
        public void TestRowMethod()
        {
            TestEZ.Test(tracer);
            TestEZ.Test(tracer);
            TestEZ.Test(tracer);
            var actual = tracer.GetTraceResult().Theards[Thread.CurrentThread.ManagedThreadId].Methods.Count;
            var expected = new MethodTraceResult();
            expected.Methods.Add(new MethodTraceResult());
            expected.Methods.Add(new MethodTraceResult());
            expected.Methods.Add(new MethodTraceResult());
            try
            {
                Assert.AreEqual(expected.Methods.Count, actual);
                logger.Info(nameof(TestRowMethod) + " - passed");
            }
            catch (Exception e)
            {
                logger.Error(e, nameof(TestRowMethod) + " - failed");
            }
            
        }
        [Test]
        public void TestRecMethod()
        {
            RecursionTest.Test(0, tracer);
            var actual = tracer.GetTraceResult().Theards[Thread.CurrentThread.ManagedThreadId].Methods[0];
            var expected = new MethodTraceResult();
            expected.MethodName = "Test";
            expected.MethodClassName = "RecursionTest";
            expected.Methods.Add(new MethodTraceResult());
            expected.Methods[0].MethodClassName = "RecursionTest";
            expected.Methods[0].MethodName = "Test";
            
            try
            {
                CheckAreEqual(expected, actual);
                CheckAreEqual(expected.Methods[0], actual.Methods[0]);
                logger.Info(nameof(TestRecMethod) + " - passed");
            }
            catch (Exception e)
            {
                logger.Error(e, nameof(TestRecMethod) + " - failed");
            }
        }
        [Test]
        public void InsertTestMethod()
        {
            InsertTest.InsertTestMethod(tracer);
            var actual = tracer.GetTraceResult().Theards[Thread.CurrentThread.ManagedThreadId].Methods[0];
            var expected = new MethodTraceResult();
            expected.MethodName = "InsertTestMethod";
            expected.MethodClassName = "InsertTest";
            expected.Methods.Add(new MethodTraceResult());
            expected.Methods[0].MethodClassName = "TestEZ";
            expected.Methods[0].MethodName = "Test";
            
            try
            {
                CheckAreEqual(expected, actual);
                CheckAreEqual(expected.Methods[0], actual.Methods[0]);
                Assert.IsNotNull(actual.Methods[0].MethodExecuteTime);
                logger.Info(nameof(InsertTestMethod) + " - passed");
            }
            catch (Exception e)
            {
                logger.Error(e, nameof(InsertTestMethod) + " - failed");
            }
        }
        [Test]
        public void InsertTestRecMethod()
        {
            InsertRecursionTest.Test(0, tracer);
            var actual = tracer.GetTraceResult().Theards[Thread.CurrentThread.ManagedThreadId].Methods[0];
            var expected = new MethodTraceResult();
            expected.MethodName = "Test";
            expected.MethodClassName = "InsertRecursionTest";
            expected.Methods.Add(new MethodTraceResult());
            expected.Methods[0].MethodClassName = "TestEZ";
            expected.Methods[0].MethodName = "Test";
            expected.Methods.Add(new MethodTraceResult());
            expected.Methods[1].MethodClassName = "InsertRecursionTest";
            expected.Methods[1].MethodName = "Test";
            expected.Methods[1].Methods.Add(new MethodTraceResult());
            expected.Methods[1].Methods[0].MethodName = "Test";
            expected.Methods[1].Methods[0].MethodClassName = "TestEZ";
            try
            {
                CheckAreEqual(expected, actual);
                CheckAreEqual(expected.Methods[0], actual.Methods[0]);
                CheckAreEqual(expected.Methods[1], actual.Methods[1]);
                CheckAreEqual(expected.Methods[1].Methods[0], actual.Methods[1].Methods[0]);

                logger.Info(nameof(InsertTestRecMethod) + " - passed");
            }
            catch (Exception e)
            {
                logger.Error(e, nameof(InsertTestRecMethod) + " - failed");
            }
        }
        [Test]
        public void TestComb()
        {
            Comb.Test(tracer);
            var actual = tracer.GetTraceResult().Theards[Thread.CurrentThread.ManagedThreadId].Methods[0];
            var expected = new MethodTraceResult();
            expected.MethodName = "Test";
            expected.MethodClassName = "Comb";
            expected.Methods.Add(new MethodTraceResult());
            expected.Methods[0].MethodClassName = "TestEZ";
            expected.Methods[0].MethodName = "Test";
            expected.Methods.Add(new MethodTraceResult());
            expected.Methods[1].MethodClassName = "RecursionTest";
            expected.Methods[1].MethodName = "Test";
            try
            {
                CheckAreEqual(expected, actual);
                CheckAreEqual(expected.Methods[0], actual.Methods[0]);
                CheckAreEqual(expected.Methods[1], actual.Methods[1]);
                logger.Info(nameof(TestComb) + " - passed");
            }
            catch (Exception e)
            {
                logger.Error(e, nameof(TestComb) + " - failed");
            }
        }
        [Test]
        public void TestException()
        {
            try
            {
                Exc.Test(tracer);
                logger.Error(nameof(TestException) + " - failed");
                Assert.Fail();
            }
            catch (Exception e)
            {
                logger.Info(e, nameof(TestException) + " - passed");
                Assert.Pass();
            }
        }
        [Test]
        public void MeasureMutualRecursionCall_int_1_Equal()
        {
            MutualRecursionTest.MutualCall(1,tracer);
            var actual = tracer.GetTraceResult().Theards[Thread.CurrentThread.ManagedThreadId].Methods[0];
            var expected = new MethodTraceResult();
            expected.MethodName = "MutualCall";
            expected.MethodClassName = "MutualRecursionTest";
            expected.Methods.Add(new MethodTraceResult());
            expected.Methods[0].MethodClassName = "MutualRecursionTest";
            expected.Methods[0].MethodName = "MutualCall2";
            expected.Methods[0].Methods.Add(new MethodTraceResult());
            expected.Methods[0].Methods[0].MethodName = "MutualCall";
            expected.Methods[0].Methods[0].MethodClassName = "MutualRecursionTest";

            try
            {
                CheckAreEqual(expected, actual);
                CheckAreEqual(expected.Methods[0], actual.Methods[0]);
                CheckAreEqual(expected.Methods[0].Methods[0], actual.Methods[0].Methods[0]);
                logger.Info(nameof(MeasureMutualRecursionCall_int_1_Equal) + " - passed");
            }
            catch (Exception e)
            {
                logger.Error(e, nameof(MeasureMutualRecursionCall_int_1_Equal) + " - failed");
            }
            
        }

        [Test]
        public void MeasureMutualRecursionCall_int_2_Equal()
        {
            MutualRecursionTest.MutualCall(2, tracer);
            var actual = tracer.GetTraceResult().Theards[Thread.CurrentThread.ManagedThreadId].Methods[0];
            var expected = new MethodTraceResult();
            expected.MethodName = "MutualCall";
            expected.MethodClassName = "MutualRecursionTest";
            expected.Methods.Add(new MethodTraceResult());
            expected.Methods[0].MethodClassName = "MutualRecursionTest";
            expected.Methods[0].MethodName = "MutualCall2";
            expected.Methods[0].Methods.Add(new MethodTraceResult());
            expected.Methods[0].Methods[0].MethodName = "MutualCall";
            expected.Methods[0].Methods[0].MethodClassName = "MutualRecursionTest";
            expected.Methods[0].Methods[0].Methods.Add(new MethodTraceResult());
            expected.Methods[0].Methods[0].Methods[0].MethodName = "MutualCall2";
            expected.Methods[0].Methods[0].Methods[0].MethodClassName = "MutualRecursionTest";
            expected.Methods[0].Methods[0].Methods[0].Methods.Add(new MethodTraceResult());
            expected.Methods[0].Methods[0].Methods[0].Methods[0].MethodName = "MutualCall";
            expected.Methods[0].Methods[0].Methods[0].Methods[0].MethodClassName = "MutualRecursionTest";

            try
            {
                CheckAreEqual(expected, actual);
                CheckAreEqual(expected.Methods[0], actual.Methods[0]);
                CheckAreEqual(expected.Methods[0].Methods[0], actual.Methods[0].Methods[0]);
                CheckAreEqual(expected.Methods[0].Methods[0].Methods[0], actual.Methods[0].Methods[0].Methods[0]);
                logger.Info(nameof(MeasureMutualRecursionCall_int_2_Equal) + " - passed");
            }
            catch (Exception e)
            {
                logger.Error(e, nameof(MeasureMutualRecursionCall_int_2_Equal) + " - failed");
            }

        }

        [Test]
        public void TestRecursionWrong()
        {
            Except.Test(tracer);
            var actual = tracer.GetTraceResult().Theards[Thread.CurrentThread.ManagedThreadId].Methods[0];
            var expected = new MethodTraceResult();
            expected.MethodName = "Test";
            expected.MethodClassName = "Except";
            expected.Methods.Add(new MethodTraceResult());
            expected.Methods[0].MethodClassName = "RecursionTest";
            expected.Methods[0].MethodName = "Test";

            try
            {
                CheckAreEqual(expected, actual);
                CheckAreEqual(expected.Methods[0], actual.Methods[0]);
                logger.Info(nameof(TestRecursionWrong) + " - passed");
            }
            catch (Exception e)
            {
                logger.Error(e, nameof(TestRecursionWrong) + " - failed");
            }
        }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Object.Build.Implementation;
using System;
using System.Diagnostics;

namespace Object.Build.Tests
{
    [TestClass]
    public class BuilderTests
    {
        readonly int TestAccountId = 123;

        readonly DateTime TestCreateTime = new DateTime(2015, 3, 4);

        [TestMethod]
        [TestCategory("Speed")]
        public void Build_SpeedTest()
        {
            var fileKey = new Builder<ImmutableFileKey>()
                .Set(k => k.AccountId, TestAccountId)
                .Set(k => k.CreateTime, TestCreateTime)
                .Build();

            var count = 0;

            var time = Stopwatch.StartNew();

            while (time.Elapsed.TotalSeconds < 5)
            {
                fileKey = new Builder<ImmutableFileKey>()
                    .Set(k => k.AccountId, TestAccountId)
                    .Set(k => k.CreateTime, TestCreateTime)
                    .Build();

                count++;
            }

            var totalMs = time.Elapsed.TotalMilliseconds;

            Console.WriteLine($"Rate: {count / (totalMs / 1000):0.00} / s, Avg: {totalMs / count:0.00} ms");
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Build_WhenImmutableAllValues_ThenSuccess()
        {
            var fileKey = new Builder<ImmutableFileKey>()
                .Set(k => k.AccountId, TestAccountId)
                .Set(k => k.CreateTime, TestCreateTime)
                .Build();

            Assert.AreEqual(TestAccountId, fileKey.AccountId);

            Assert.AreEqual(TestCreateTime, fileKey.CreateTime);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Build_WhenImmutableSomeValues_ThenSuccess()
        {
            var fileKey = new Builder<ImmutableFileKey>()
                .Set(k => k.AccountId, TestAccountId)
                .Build();

            Assert.AreEqual(TestAccountId, fileKey.AccountId);

            Assert.IsNull(fileKey.CreateTime);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Build_WhenSemiMutableAllValues_ThenSuccess()
        {
            var fileKey = new Builder<SemiMutableFileKey>()
                .Set(k => k.AccountId, TestAccountId)
                .Set(k => k.CreateTime, TestCreateTime)
                .Build();

            Assert.AreEqual(TestAccountId, fileKey.AccountId);

            Assert.AreEqual(TestCreateTime, fileKey.CreateTime);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Build_WhenSemiMutableSomeValues_ThenSuccess()
        {
            var fileKey = new Builder<SemiMutableFileKey>()
                .Set(k => k.CreateTime, TestCreateTime)
                .Build();

            Assert.IsNull(fileKey.AccountId);

            Assert.AreEqual(TestCreateTime, fileKey.CreateTime);            
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Build_WhenMutableAllValues_ThenSuccess()
        {
            var fileKey = new Builder<MutableFileKey>()
                .Set(k => k.AccountId, TestAccountId)
                .Set(k => k.CreateTime, TestCreateTime)
                .Build();

            Assert.AreEqual(TestAccountId, fileKey.AccountId);

            Assert.AreEqual(TestCreateTime, fileKey.CreateTime);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Build_WhenMutableSomeValues_ThenSuccess()
        {
            var fileKey = new Builder<MutableFileKey>()
                .Set(k => k.AccountId, TestAccountId)
                .Build();

            Assert.AreEqual(TestAccountId, fileKey.AccountId);

            Assert.IsNull(fileKey.CreateTime);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Build_WhenClone_ThenSuccess()
        {
            var fileKeyA = new ImmutableFileKey(TestAccountId, TestCreateTime);

            var fileKeyB = new Builder<ImmutableFileKey>(fileKeyA).Build();

            Assert.AreEqual(fileKeyA.AccountId, fileKeyB.AccountId);

            Assert.AreEqual(fileKeyA.CreateTime, fileKeyB.CreateTime);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Build_WhenMutation_ThenSuccess()
        {
            var newAccountId = 234;

            var fileKeyA = new ImmutableFileKey(TestAccountId, TestCreateTime);

            var fileKeyB = new Builder<ImmutableFileKey>(fileKeyA)
                .Set(k => k.AccountId, newAccountId)
                .Build();

            Assert.AreEqual(newAccountId, fileKeyB.AccountId);

            Assert.AreEqual(fileKeyA.CreateTime, fileKeyB.CreateTime);
        }

        public class ImmutableFileKey
        {
            public ImmutableFileKey(
                int? accountId, 
                DateTime? createTime)
            {
                AccountId = accountId;

                CreateTime = createTime;
            }

            public int? AccountId { get; }

            public DateTime? CreateTime { get; }
        }

        public class SemiMutableFileKey
        {
            public SemiMutableFileKey(
                DateTime? createTime)
            {
                CreateTime = createTime;
            }

            public int? AccountId { get; set; }

            public DateTime? CreateTime { get; }
        }

        public class MutableFileKey
        {
            public int? AccountId { get; set; }

            public DateTime? CreateTime { get; set; }
        }
    }
}

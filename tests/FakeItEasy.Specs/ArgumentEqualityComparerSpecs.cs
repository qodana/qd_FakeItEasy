﻿namespace FakeItEasy.Specs
{
    using System;
    using FluentAssertions;
    using Xbehave;
    using Xunit;

    public static class ArgumentEqualityComparerSpecs
    {
        [Scenario]
        public static void CustomArgumentEqualityComparer(IFoo fake, int result)
        {
            "Given a fake"
                .x(() => fake = A.Fake<IFoo>());

            "And a type for which a custom argument equality comparer exists"
                .See<ClassWithCustomArgumentEqualityComparer>();

            "When a call to the fake is configured with a specific argument value"
                .x(() => A.CallTo(() => fake.Bar(new ClassWithCustomArgumentEqualityComparer { Value = 1 })).Returns(42));

            "And a call to the fake is made with a distinct but identical instance"
                .x(() => result = fake.Bar(new ClassWithCustomArgumentEqualityComparer { Value = 1 }));

            "Then it should return the configured value"
                .x(() => result.Should().Be(42));
        }

        [Scenario]
        public static void TwoCustomArgumentEqualityComparers(IFoo fake, int result)
        {
            "Given a fake"
                .x(() => fake = A.Fake<IFoo>());

            "And a type for which two custom argument equality comparers exist"
                .See<ClassWithTwoEligibleArgumentEqualityComparers>();

            "When a call to the fake is configured with a specific argument value"
                .x(() => A.CallTo(() => fake.Baz(new ClassWithTwoEligibleArgumentEqualityComparers { X = 1, Y = 1 })).Returns(42));

            "And a call to the fake is made with a distinct but identical instance according to the higher-priority comparer"
                .x(() => result = fake.Baz(new ClassWithTwoEligibleArgumentEqualityComparers { X = 0, Y = 1 }));

            "Then it should return the configured value"
                .x(() => result.Should().Be(42));
        }

        [Scenario]
        public static void ArgumentEqualityComparerThatThrows(IFoo fake, Exception? exception)
        {
            "Given a fake"
                .x(() => fake = A.Fake<IFoo>());

            "And a type for which there's an argument equality comparer that throws"
                .See<ClassWithEqualityComparerThatThrows>();

            "When a call to the fake is configured with a specific argument value"
                .x(() => A.CallTo(() => fake.Frob(new ClassWithEqualityComparerThatThrows())).Returns(42));

            "And a call to the fake is made"
                .x(() => exception = Record.Exception(() => fake.Frob(new ClassWithEqualityComparerThatThrows())));

            "Then it should throw a UserCallbackException with a message explaining what happened"
                .x(() => exception.Should().BeOfType<UserCallbackException>()
                    .Which.Message.Should().Be("Argument Equality Comparer threw an exception. See inner exception for details."));

            "And the exception should wrap the original exception"
                .x(() => exception!.InnerException.Should().BeOfType<Exception>()
                    .Which.Message.Should().Be("Oops"));
        }

        public interface IFoo
        {
            int Bar(ClassWithCustomArgumentEqualityComparer arg);

            int Baz(ClassWithTwoEligibleArgumentEqualityComparers arg);

            int Frob(ClassWithEqualityComparerThatThrows arg);
        }

        public class ClassWithCustomArgumentEqualityComparer
        {
            public int Value { get; set; }
        }

        public class CustomComparer : ArgumentEqualityComparer<ClassWithCustomArgumentEqualityComparer>
        {
            protected override bool AreEqual(
                ClassWithCustomArgumentEqualityComparer? expectedValue,
                ClassWithCustomArgumentEqualityComparer? argumentValue)
            {
                return expectedValue?.Value == argumentValue?.Value;
            }
        }

        public class ClassWithTwoEligibleArgumentEqualityComparers
        {
            public int X { get; set; }

            public int Y { get; set; }
        }

        public class XComparer : ArgumentEqualityComparer<ClassWithTwoEligibleArgumentEqualityComparers>
        {
            public override Priority Priority => new Priority(1);

            protected override bool AreEqual(
                ClassWithTwoEligibleArgumentEqualityComparers? expectedValue,
                ClassWithTwoEligibleArgumentEqualityComparers? argumentValue)
            {
                return expectedValue?.X == argumentValue?.X;
            }
        }

        public class YComparer : ArgumentEqualityComparer<ClassWithTwoEligibleArgumentEqualityComparers>
        {
            public override Priority Priority => new Priority(2);

            protected override bool AreEqual(
                ClassWithTwoEligibleArgumentEqualityComparers? expectedValue,
                ClassWithTwoEligibleArgumentEqualityComparers? argumentValue)
            {
                return expectedValue?.Y == argumentValue?.Y;
            }
        }

        public class ClassWithEqualityComparerThatThrows
        {
        }

        public class ComparerThatThrows : ArgumentEqualityComparer<ClassWithEqualityComparerThatThrows>
        {
            protected override bool AreEqual(
                ClassWithEqualityComparerThatThrows? expectedValue,
                ClassWithEqualityComparerThatThrows? argumentValue)
            {
                throw new Exception("Oops");
            }
        }
    }
}
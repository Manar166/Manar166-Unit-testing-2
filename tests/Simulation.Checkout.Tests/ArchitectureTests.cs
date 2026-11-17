using FluentAssertions;
using NetArchTest.Rules;
using Simulation.Checkout.Application;
using Simulation.Checkout.Application.Abstractions;
using Simulation.Checkout.Domain;
using Simulation.Checkout.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simulation.Checkout.Tests
{
    public class ArchitectureTests
    {
        [Fact]
        public void Domain_ShouldNotDependOnApplicationOrInfrastructure()

        {
            //arrange
            var domainAssembely = typeof(Order).Assembly;

            var rule = Types.InAssembly(domainAssembely)
                .ShouldNot()
                .HaveDependencyOnAny("Simulation.Checkout.Application",
                                  "Simulation.Checkout.Infrastructure");

            //act ,assert
            rule.GetResult().IsSuccessful.Should().BeTrue();
        }


        [Fact]
        public void Application_ShouldNotDependOnInfrastructure()
        {
            //arrange
            var applicationAssembely = typeof(Application.CheckoutService).Assembly;

            var rule = Types.InAssembly(applicationAssembely).ShouldNot().HaveDependencyOn("Simulation.Checkout.Infrastructure");

            //act .assert
            rule.GetResult().IsSuccessful.Should().BeTrue();

        }


        [Fact]

        public void AllInterfaces_ShouldStartWithI()
        {
            var assemblies = new[]
            { typeof(CheckoutService).Assembly,
              typeof(Order).Assembly,
              typeof(InMemoryOrderRepository).Assembly
            };


            var results=assemblies.Select(asembely => Types.InAssembly(asembely)
            .That()
            .AreInterfaces()
                .Should()
                .HaveNameStartingWith("I")
                .GetResult())
                .ToList();


            results.Should().OnlyContain(result => result.IsSuccessful);

        }

        [Fact]

        public void ClassesImplementingIOrderRepository_ShouldEndWithRepository()
        {
            var assembly = typeof(InMemoryOrderRepository).Assembly;



            var result = Types.InAssembly(assembly)
            .That().AreClasses().And().ImplementInterface(typeof(IOrderRepository))
            .Should().HaveNameEndingWith("Repository");
            


            result.GetResult().IsSuccessful.Should().Be(true);

        }

    }
}

using Bogus;
using FluentAssertions;
using Moq;
using Simulation.Checkout.Application;
using Simulation.Checkout.Application.Abstractions;
using Simulation.Checkout.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simulation.Checkout.Tests.TestData
{
    public class BogusHelpers
    {
        [Fact]
        public async Task CheckoutAsync_WhenCustomerGeneratedByBogus_ShouldPreserveCustomerDetails()
        {
            //arrange
            var faker = new Faker<Order>().UseSeed(2026)
                        .RuleFor(o => o.CustomerName, f => f.Name.FullName())
                        .RuleFor(o => o.CustomerEmail, f => f.Internet.Email())
                        .RuleFor(o => o.Items, f => new List<OrderItem>
                        {
                            new OrderItem
                            {
                                ProductId = Guid.NewGuid(),
                                ProductName = f.Commerce.ProductName(),
                                Quantity = f.Random.Int(1, 5),
                                UnitPrice = f.Random.Decimal(10m, 100m)
                            }
                        });


            var order = faker.Generate();

            var repositoryMock = new Mock<IOrderRepository>();
            var inventoryMock = new Mock<IInventoryService>();
            var notificationMock = new Mock<INotificationService>();
            inventoryMock
                       .Setup(x => x.IsAvailableAsync(
                           It.IsAny<Guid>(),
                           It.IsAny<int>(),
                           It.IsAny<CancellationToken>()))
                       .ReturnsAsync(true);

            var checkoutService = new CheckoutService(repositoryMock.Object, inventoryMock.Object, notificationMock.Object);

            //act
            var receipt = await checkoutService.CheckoutAsync(order);

            //assert

            receipt.CustomerName.Should().Be(order.CustomerName);
            receipt.CustomerEmail.Should().Be(order.CustomerEmail);




        }

    }
}

using AutoFixture;
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
    public class AutoFixture
    {
        [Fact]
        public async Task CheckoutAsync_WhenOrderCreatedByAutoFixture_ShouldSucceed()
        { //arrange
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

            Fixture fixture = new Fixture();
            fixture.Customize<OrderItem>(i => i.With(x => x.Quantity, 2).With(x => x.UnitPrice, 500m));

            var order = fixture.Create<Order>();


            //act
            var receipt = await checkoutService.CheckoutAsync(order);

            //assert

            receipt.Should().NotBeNull();
            receipt.OrderId.Should().Be(order.Id);
            receipt.Items.Should().HaveCount(order.Items.Count);
            receipt.Total.Should().BeGreaterThanOrEqualTo(0m);

        }
    }
}

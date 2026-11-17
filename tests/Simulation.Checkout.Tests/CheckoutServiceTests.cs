using AutoFixture;
using Bogus;
using FluentAssertions;
using Moq;
using Simulation.Checkout.Application;
using Simulation.Checkout.Application.Abstractions;
using Simulation.Checkout.Domain;
using Simulation.Checkout.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simulation.Checkout.Tests
{
    public class CheckoutServiceTests
    {
        [Fact]
        public async Task CheckoutAsync_WhenOrderIsEmpty_ShouldThrowEmptyOrderException()
        {
            var repositoryMock = new Mock<IOrderRepository>();
            var inventoryMock = new Mock<IInventoryService>();
            var notificationMock = new Mock<INotificationService>();

            var checkoutService = new CheckoutService(repositoryMock.Object, inventoryMock.Object, notificationMock.Object);

            var order = new Order
            {
                Id = Guid.NewGuid(),
                CustomerName = "manar",
                CustomerEmail = "manar@example.com",
                Items = new List<OrderItem>()
            };


            Func<Task> act = () => checkoutService.CheckoutAsync(order);

            await act.Should().ThrowAsync<EmptyOrderException>();




        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task CheckoutAsync_WhenQuantityIsInvalid_ShouldThrowInvalidQuantityException(int quantity)
        {
            var repositoryMock = new Mock<IOrderRepository>();
            var inventoryMock = new Mock<IInventoryService>();
            var notificationMock = new Mock<INotificationService>();

            var checkoutService = new CheckoutService(repositoryMock.Object, inventoryMock.Object, notificationMock.Object);

            var order = new Order
            {
                Id = Guid.NewGuid(),
                CustomerName = "manar",
                CustomerEmail = "manar@example.com",
                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                    ProductId =new Guid(),
                    ProductName = "Laptop",
                    Quantity = quantity,
                    UnitPrice = 500m
                    }
                }
            };

            Func<Task> act = () => checkoutService.CheckoutAsync(order);

            await act.Should().ThrowAsync<InvalidQuantityException>().WithMessage("*Laptop*");




        }


        [Fact]
        public async Task CheckoutAsync_WhenPriceIsNegative_ShouldThrowInvalidPriceException()
        {
            var repositoryMock = new Mock<IOrderRepository>();
            var inventoryMock = new Mock<IInventoryService>();
            var notificationMock = new Mock<INotificationService>();

            var checkoutService = new CheckoutService(repositoryMock.Object, inventoryMock.Object, notificationMock.Object);

            var order = new Order
            {
                Id = Guid.NewGuid(),
                CustomerName = "manar",
                CustomerEmail = "manar@example.com",
                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ProductId = new Guid(),
                        ProductName = "Laptop",
                        Quantity = 1,
                        UnitPrice = -0.01m
                    }
                }
            };

            Func<Task> act = () => checkoutService.CheckoutAsync(order);

            await act.Should().ThrowAsync<InvalidPriceException>();

        }


        [Fact]
        public async Task CheckOutAsync_WhenPriceIsZero_ShouldSuccess()
        {
            var repositoryMock = new Mock<IOrderRepository>();
            var inventoryMock = new Mock<IInventoryService>();
            var notificationMock = new Mock<INotificationService>();

            inventoryMock.Setup(x => x.IsAvailableAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            var checkoutService = new CheckoutService(repositoryMock.Object, inventoryMock.Object, notificationMock.Object);

            var order = new Order
            {
                Id = Guid.NewGuid(),
                CustomerName = "manar",
                CustomerEmail = "manar@example.com",
                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ProductId = new Guid(),
                        ProductName = "Laptop",
                        Quantity = 1,
                        UnitPrice = 0m
                    }
                }
            };

            var receipt = await checkoutService.CheckoutAsync(order);

            receipt.Should().NotBeNull();
            receipt.Total.Should().Be(0m);
            receipt.Discount.Should().Be(0m);
            receipt.Subtotal.Should().Be(0m);
            receipt.Subtotal.Should().Be(0m);

        }



        [Fact]
        public async Task CheckoutAsync_WhenStockUnavialable_ShouldThrowInsufficientInventoryException()
        {
            var repositoryMock = new Mock<IOrderRepository>();
            var inventoryMock = new Mock<IInventoryService>();
            var notificationMock = new Mock<INotificationService>();


            var availableID = Guid.NewGuid();
            var unAvailableID = Guid.NewGuid();

            inventoryMock.Setup(x => x.IsAvailableAsync(availableID, 1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            inventoryMock.Setup(x => x.IsAvailableAsync(unAvailableID, 1, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var checkoutService = new CheckoutService(repositoryMock.Object, inventoryMock.Object, notificationMock.Object);

            var order = new Order
            {
                Id = Guid.NewGuid(),
                CustomerName = "Manar",
                CustomerEmail = "manar@example.com",
                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ProductId = availableID,
                        ProductName = "Laptop",
                        Quantity = 1,
                        UnitPrice = 500m
                    },
                    new OrderItem
                    {
                        ProductId = unAvailableID,
                        ProductName = "Mouse",
                        Quantity = 1,
                        UnitPrice = 100m
                    }
                }
            };


            Func<Task> act = () => checkoutService.CheckoutAsync(order);
            await act.Should().ThrowAsync<InsufficientInventoryException>().WithMessage("*Mouse*");



        }

        [Fact]
        public async Task CheckoutAsync_WithMultipleItems_ShouldCalculateCorrectSubtotal()
        {
            var repositoryMock = new Mock<IOrderRepository>();
            var inventoryMock = new Mock<IInventoryService>();
            var notificationMock = new Mock<INotificationService>();


            inventoryMock.Setup(x => x.IsAvailableAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var checkoutService = new CheckoutService(
                                                      repositoryMock.Object,
                                                      inventoryMock.Object,
                                                      notificationMock.Object);

            var order = new Order
            {
                Id = Guid.NewGuid(),
                CustomerName = "Manar",
                CustomerEmail = "manar@example.com",
                Items = new List<OrderItem>
        {
            new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Laptop",
                UnitPrice = 500m,
                Quantity = 2
            },
            new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Mouse",
                UnitPrice = 100m,
                Quantity = 3
            },
            new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Keyboard",
                UnitPrice = 200m,
                Quantity = 1
            }
        }
            };

            var receipt = await checkoutService.CheckoutAsync(order);
            receipt.Subtotal.Should().Be(1500m);

        }


        public static TheoryData<decimal, decimal> data => new()
        {
            {999.99m,0m },
            {1000m,100m },
            {1500m,100m }

        };

        [Theory]
        [MemberData(nameof(data))]
        public async Task CheckoutAsync_WithDifferentSubtotals_ShouldApplyCorrectDiscount(decimal subTotal, decimal expectedDiscount)
        {
            var repository = new Mock<IOrderRepository>();
            var inventory = new Mock<IInventoryService>();
            var notification = new Mock<INotificationService>();

            inventory.Setup(x => x.IsAvailableAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var checkOutService = new CheckoutService(repository.Object, inventory.Object, notification.Object);

            var order = new Order
            {
                Id = Guid.NewGuid(),
                CustomerName = "Manar",
                CustomerEmail = "manar@example.com",
                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ProductId = Guid.NewGuid(),
                        ProductName = "Laptop",
                        Quantity = 1,
                        UnitPrice = subTotal
                    }
                }
            };

            var reciept = await checkOutService.CheckoutAsync(order);
            reciept.Discount.Should().Be(expectedDiscount);


        }





        public static TheoryData<Order, decimal, decimal, decimal> CheckoutCases => new()
    {
        {
            new Order
            {
                Id = Guid.NewGuid(),
                CustomerName = "Manar",
                CustomerEmail = "manar@example.com",
                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ProductId = Guid.NewGuid(),
                        ProductName = "Laptop",
                        Quantity = 2,
                        UnitPrice = 150m
                    },
                    new OrderItem
                    {
                        ProductId = Guid.NewGuid(),
                        ProductName = "Mouse",
                        Quantity = 1,
                        UnitPrice = 400m
                    }
                }
            },
            700m, 0m, 700m
        },
        {
            new Order
            {
                Id = Guid.NewGuid(),
                CustomerName = "Manar",
                CustomerEmail = "manar@example.com",
                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ProductId = Guid.NewGuid(),
                        ProductName = "Laptop",
                        Quantity = 3,
                        UnitPrice = 500m
                    }
                }
            },
            1500m, 100m, 1400m
        },
        {
            new Order
            {
                Id = Guid.NewGuid(),
                CustomerName = "Manar",
                CustomerEmail = "manar@example.com",
                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ProductId = Guid.NewGuid(),
                        ProductName = "Laptop",
                        Quantity = 1,
                        UnitPrice = 999.99m
                    }
                }
            },
            999.99m, 0m, 999.99m
        }
    };

        [Theory]
        [MemberData(nameof(CheckoutCases))]
        public async Task CheckoutAsync_WithExampleOrders_ShouldCalculateCorrectTotals
                                                    (Order order, decimal expectedSubTotal, decimal expectedDiscount, decimal expectedTotal)
        {
            var repositoryMock = new Mock<IOrderRepository>();
            var inventoryMock = new Mock<IInventoryService>();
            var notificationMock = new Mock<INotificationService>();
            inventoryMock.Setup(x => x.IsAvailableAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            var checkoutService = new CheckoutService(repositoryMock.Object, inventoryMock.Object, notificationMock.Object);
            var receipt = await checkoutService.CheckoutAsync(order);
            receipt.Subtotal.Should().Be(expectedSubTotal);
            receipt.Discount.Should().Be(expectedDiscount);
            receipt.Total.Should().Be(expectedTotal);
        }


        [Fact]
        public async Task CheckoutAsync_WhenOrderIsValid_ShouldSaveAndSendConfirmationOnce()
        {
            var repositoryMock = new Mock<IOrderRepository>();
            var inventoryMock = new Mock<IInventoryService>();
            var notificationMock = new Mock<INotificationService>();

            inventoryMock.Setup(x => x.IsAvailableAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            var checkoutService = new CheckoutService(repositoryMock.Object, inventoryMock.Object, notificationMock.Object);

            var order = new Order
            {
                Id = Guid.NewGuid(),
                CustomerName = "Manar",
                CustomerEmail = "manar@example.com",
                Items = new List<OrderItem>
                        {
                            new OrderItem
                            {
                                ProductId = Guid.NewGuid(),
                                ProductName = "Laptop",
                                Quantity = 1,
                                UnitPrice = 500m
                            }
                        }
            };

            var receipt = await checkoutService.CheckoutAsync(order);
            receipt.Should().NotBeNull();
            receipt.OrderId.Should().Be(order.Id);

            repositoryMock.Verify(x => x.SaveAsync(order, It.IsAny<CancellationToken>()), Times.Once());
            notificationMock.Verify(x => x.SendOrderConfirmationAsync(receipt, It.IsAny<CancellationToken>()), Times.Once());









        }

        [Fact]
        public async Task CheckoutAsync_WhenItemIsOutOfStock_ShouldNotSaveAndNotSendConfirmationOnce()
        {
            var repositoryMock = new Mock<IOrderRepository>();
            var inventoryMock = new Mock<IInventoryService>();
            var notificationMock = new Mock<INotificationService>();

            var laptopId = Guid.NewGuid();
            var mouseId = Guid.NewGuid();

            inventoryMock.Setup(x => x.IsAvailableAsync(laptopId, It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            inventoryMock.Setup(x => x.IsAvailableAsync(mouseId, It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
            var checkoutService = new CheckoutService(repositoryMock.Object, inventoryMock.Object, notificationMock.Object);

            var order = new Order
            {
                Id = Guid.NewGuid(),
                CustomerName = "Manar",
                CustomerEmail = "manar@example.com",
                Items = new List<OrderItem>
                        {
                            new OrderItem
                            {
                                ProductId = laptopId,
                                ProductName = "Laptop",
                                Quantity = 1,
                                UnitPrice = 500m
                            },
                            new OrderItem
                            {
                                ProductId = mouseId,
                                ProductName = "Mouse",
                                Quantity = 2,
                                UnitPrice = 100m
                            }
                        }
            };

            Func<Task> act = () => checkoutService.CheckoutAsync(order);
            await act.Should().ThrowAsync<InsufficientInventoryException>().WithMessage("*Mouse*");

            repositoryMock.Verify(x => x.SaveAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never());
            notificationMock.Verify(x => x.SendOrderConfirmationAsync(It.IsAny<CheckoutReceipt>(), It.IsAny<CancellationToken>()), Times.Never());











        }


        [Fact]
        public async Task CheckoutAsync_WhenValidationFails_ShouldNotCallDependencies()
        {
            //arrange
            var repositoryMock = new Mock<IOrderRepository>();
            var inventoryMock = new Mock<IInventoryService>();
            var notificationMock = new Mock<INotificationService>();

            var checkoutService = new CheckoutService(repositoryMock.Object, inventoryMock.Object, notificationMock.Object);

            var order = new Order
            {
                Id = Guid.NewGuid(),
                CustomerName = "Manar",
                CustomerEmail = "manar@example.com",
                Items = new List<OrderItem>
                        {
                            new OrderItem
                            {
                                ProductId = Guid.NewGuid(),
                                ProductName = "Laptop",
                                Quantity = 0,
                                UnitPrice = 500m
                            }
                        }
            };


            //act

            Func<Task> act=() => checkoutService.CheckoutAsync(order);
            //assert
            await act.Should().ThrowAsync<InvalidQuantityException>().WithMessage("*Laptop*");

                inventoryMock.Verify(
                               x => x.IsAvailableAsync(
                                   It.IsAny<Guid>(),
                                   It.IsAny<int>(),
                                   It.IsAny<CancellationToken>()),
                               Times.Never());

            repositoryMock.Verify(
                            x => x.SaveAsync(
                                It.IsAny<Order>(),
                                It.IsAny<CancellationToken>()),
                            Times.Never());

            notificationMock.Verify(
                            x => x.SendOrderConfirmationAsync(
                                It.IsAny<CheckoutReceipt>(),
                                It.IsAny<CancellationToken>()),
                            Times.Never());

        }

       
       


        

    }
}

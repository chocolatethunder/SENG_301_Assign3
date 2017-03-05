using System;
using System.Collections.Generic;
using Frontend2;
using Frontend2.Hardware;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest {

    [TestClass]
    public class QATest {

        private List<IDeliverable> delivered = new List<IDeliverable>();

        private List<PopCan> expectedPops;
        private int expectedChange;

        private int expectedCoinsinRack;
        private int expectedCoinsinStorage;

        private VendingMachineStoredContents unloaded = new VendingMachineStoredContents();
        private VendingMachineFactory vmf = new VendingMachineFactory();

        List<int> coinKinds;
        List<string> popNames;
        List<int> popCosts;

        [TestMethod]
        /* T01 - This test inserts the exact amount of change into the vending 
         * machine to buy a pop and dismantles it after.
         */
        public void t01_ExactChangeToBuy() {

            coinKinds = new List<int>{ 5, 10, 25, 100 };
            popNames = new List<string> { "Coke", "water", "stuff" };
            popCosts = new List<int> { 250, 250, 205};

            // Configure the machine
            int vmIndex = vmf.CreateVendingMachine(coinKinds, 3, 10, 10, 10);
            vmf.ConfigureVendingMachine(vmIndex, popNames, popCosts);

            // Load coins in the coin racks
            List<List<Coin>> coinRacks = new List<List<Coin>>();
            coinRacks.Add(new List<Coin> { new Coin(5) });                      // Rack 0
            coinRacks.Add(new List<Coin> { new Coin(10) });                     // Rack 1
            coinRacks.Add(new List<Coin> { new Coin(25), new Coin(25) });       // Rack 2
            this.LoadCoinRacks(vmIndex, coinRacks);

            // Load pops in the pop racks
            List<List<PopCan>> popRacks = new List<List<PopCan>>();
            popRacks.Add(new List<PopCan> { new PopCan("Coke") });  // Rack 0
            popRacks.Add(new List<PopCan> { new PopCan("water") }); // Rack 1
            popRacks.Add(new List<PopCan> { new PopCan("stuff") }); // Rack 2
            this.LoadPopRacks(vmIndex, popRacks);

            // Insert Coins
            List<int> coinInput = new List<int> {100,100,25,25};
            this.insertCoins(vmIndex, coinInput);

            // Make a selection
            vmf.PressButton(vmIndex, 0);

            // --- Assert Check Delivery
            // Reality
            delivered = vmf.ExtractFromDeliveryChute(vmIndex);
            // Expected
            expectedChange = 0;
            expectedPops = new List<PopCan> { new PopCan("Coke") };
            // Assert
            Assert.IsTrue(checkDelivery(expectedChange, expectedPops, delivered));

            // --- Assert Check Teardown
            // Reality
            unloaded = vmf.UnloadVendingMachine(vmIndex);
            // Expected
            expectedCoinsinRack = 315;
            expectedCoinsinStorage = 0;
            expectedPops = new List<PopCan> { new PopCan("stuff"), new PopCan("water") };
            // Assert
            Assert.IsTrue(checkTearDown(expectedCoinsinRack, expectedCoinsinStorage, expectedPops, unloaded));

        }


        [TestMethod]
        /* T02 - This test inserts an extra amount of coins and checks to see if right
         * change has been returned. It does not check for coins in Storage Bin
         */
        public void t02_ChangeAsExpected() {

            coinKinds = new List<int> { 5, 10, 25, 100 };
            popNames = new List<string> { "Coke", "water", "stuff" };
            popCosts = new List<int> { 250, 250, 205 };

            // Configure the machine
            int vmIndex = vmf.CreateVendingMachine(coinKinds, 3, 10, 10, 10);
            vmf.ConfigureVendingMachine(vmIndex, popNames, popCosts);

            // Load coins in the coin racks
            List<List<Coin>> coinRacks = new List<List<Coin>>();
            coinRacks.Add(new List<Coin> { new Coin(5) });                      // Rack 0
            coinRacks.Add(new List<Coin> { new Coin(10) });                     // Rack 1
            coinRacks.Add(new List<Coin> { new Coin(25), new Coin(25) });       // Rack 2
            this.LoadCoinRacks(vmIndex, coinRacks);

            // Load pops in the pop racks
            List<List<PopCan>> popRacks = new List<List<PopCan>>();
            popRacks.Add(new List<PopCan> { new PopCan("Coke") });  // Rack 0
            popRacks.Add(new List<PopCan> { new PopCan("water") }); // Rack 1
            popRacks.Add(new List<PopCan> { new PopCan("stuff") }); // Rack 2
            this.LoadPopRacks(vmIndex, popRacks);

            // Insert Coins
            List<int> coinInput = new List<int> { 100, 100, 100 };
            this.insertCoins(vmIndex, coinInput);

            // Make a selection
            vmf.PressButton(vmIndex, 0);

            // --- Assert Check Delivery
            // Reality
            delivered = vmf.ExtractFromDeliveryChute(vmIndex);
            // Expected
            expectedChange = 50;
            expectedPops = new List<PopCan> { new PopCan("Coke") };
            // Assert
            Assert.IsTrue(checkDelivery(expectedChange, expectedPops, delivered));

            // --- Assert Check Teardown
            // Reality
            unloaded = vmf.UnloadVendingMachine(vmIndex);
            // Expected
            expectedCoinsinRack = 315;
            expectedCoinsinStorage = 0;
            expectedPops = new List<PopCan> { new PopCan("stuff"), new PopCan("water") };
            // Assert
            Assert.IsTrue(checkTearDown(expectedCoinsinRack, expectedCoinsinStorage, expectedPops, unloaded));

        }


        [TestMethod]
        /* T03 - This test initializes a vending machine but does not load any coins or 
         * pops into it. It unloads it and then checks the teardown items. Which should be none.
         */
        public void t03_CheckTearDownWithNoConfigure() {

            coinKinds = new List<int> { 5, 10, 25, 100 };

            // Configure the machine
            int vmIndex = vmf.CreateVendingMachine(coinKinds, 3, 10, 10, 10);

            // --- Assert Check Delivery
            // Reality
            delivered = vmf.ExtractFromDeliveryChute(vmIndex);
            // Expected
            expectedChange = 0;
            expectedPops = new List<PopCan>();
            // Assert
            Assert.IsTrue(checkDelivery(expectedChange, expectedPops, delivered));

            // --- Assert Check Teardown
            // Reality
            unloaded = vmf.UnloadVendingMachine(vmIndex);
            // Expected
            expectedCoinsinRack = 0;
            expectedCoinsinStorage = 0;
            expectedPops = new List<PopCan>();
            // Assert
            Assert.IsTrue(checkTearDown(expectedCoinsinRack, expectedCoinsinStorage, expectedPops, unloaded));

        }

        [TestMethod]
        /* T04 - This tests what happens when a user doesn't enter any money and presses a selection button.
         * Nothing should happen. Nothing in delivery chute. 
         */
        public void t04_ExtractWithNoMoneyInserted() {

            coinKinds = new List<int> { 5, 10, 25, 100 };
            popNames = new List<string> { "Coke", "water", "stuff" };
            popCosts = new List<int> { 250, 250, 205 };

            // Configure the machine
            int vmIndex = vmf.CreateVendingMachine(coinKinds, 3, 10, 10, 10);
            vmf.ConfigureVendingMachine(vmIndex, popNames, popCosts);

            // Load coins in the coin racks
            List<List<Coin>> coinRacks = new List<List<Coin>>();
            coinRacks.Add(new List<Coin> { new Coin(5) });                      // Rack 0
            coinRacks.Add(new List<Coin> { new Coin(10) });                     // Rack 1
            coinRacks.Add(new List<Coin> { new Coin(25), new Coin(25) });       // Rack 2
            this.LoadCoinRacks(vmIndex, coinRacks);

            // Load pops in the pop racks
            List<List<PopCan>> popRacks = new List<List<PopCan>>();
            popRacks.Add(new List<PopCan> { new PopCan("Coke") });  // Rack 0
            popRacks.Add(new List<PopCan> { new PopCan("water") }); // Rack 1
            popRacks.Add(new List<PopCan> { new PopCan("stuff") }); // Rack 2
            this.LoadPopRacks(vmIndex, popRacks);

            // Make a selection
            vmf.PressButton(vmIndex, 0);

            // --- Assert Check Delivery
            // Reality
            delivered = vmf.ExtractFromDeliveryChute(vmIndex);
            // Expected
            expectedChange = 0;
            expectedPops = new List<PopCan>();
            // Assert
            Assert.IsTrue(checkDelivery(expectedChange, expectedPops, delivered));

            // --- Assert Check Teardown
            // Reality
            unloaded = vmf.UnloadVendingMachine(vmIndex);
            // Expected
            expectedCoinsinRack = 65;
            expectedCoinsinStorage = 0;
            expectedPops = new List<PopCan> { new PopCan("Coke"), new PopCan("stuff"), new PopCan("water") };
            // Assert
            Assert.IsTrue(checkTearDown(expectedCoinsinRack, expectedCoinsinStorage, expectedPops, unloaded));

        }


        [TestMethod]
        /* T05 - The coin racks are not configured in an increasing order. This checks the affiliation of 
         * each coin type to its coinKindIndex in the machine. 
         */
        public void t05_CoinsLoadedIntoIncorrectRacks() {

            coinKinds = new List<int> { 100, 5, 25, 10 };
            popNames = new List<string> { "Coke", "water", "stuff" };
            popCosts = new List<int> { 250, 250, 205 };

            // Configure the machine
            int vmIndex = vmf.CreateVendingMachine(coinKinds, 3, 2, 10, 10);
            vmf.ConfigureVendingMachine(vmIndex, popNames, popCosts);

            // Load coins in the coin racks
            List<List<Coin>> coinRacks = new List<List<Coin>>();
            coinRacks.Add(new List<Coin>());                                    // Rack 0
            coinRacks.Add(new List<Coin> { new Coin(5) });                      // Rack 1
            coinRacks.Add(new List<Coin> { new Coin(25), new Coin(25) });       // Rack 2
            coinRacks.Add(new List<Coin> { new Coin(10) });                     // Rack 3
            this.LoadCoinRacks(vmIndex, coinRacks);

            // Load pops in the pop racks
            List<List<PopCan>> popRacks = new List<List<PopCan>>();
            popRacks.Add(new List<PopCan> { new PopCan("Coke") });  // Rack 0
            popRacks.Add(new List<PopCan> { new PopCan("water") }); // Rack 1
            popRacks.Add(new List<PopCan> { new PopCan("stuff") }); // Rack 2
            this.LoadPopRacks(vmIndex, popRacks);

            // Insert Coins
            List<int> coinInput = new List<int> { 100, 100, 100 };
            this.insertCoins(vmIndex, coinInput);

            // Make a selection
            vmf.PressButton(vmIndex, 0);

            // --- Assert Check Delivery
            // Reality
            delivered = vmf.ExtractFromDeliveryChute(vmIndex);
            // Expected
            expectedChange = 50;
            expectedPops = new List<PopCan> { new PopCan("Coke") };
            // Assert
            Assert.IsTrue(checkDelivery(expectedChange, expectedPops, delivered));

            // --- Assert Check Teardown
            // Reality
            unloaded = vmf.UnloadVendingMachine(vmIndex);
            // Expected
            expectedCoinsinRack = 215;
            expectedCoinsinStorage = 100;
            expectedPops = new List<PopCan> { new PopCan("stuff"), new PopCan("water") };
            // Assert
            Assert.IsTrue(checkTearDown(expectedCoinsinRack, expectedCoinsinStorage, expectedPops, unloaded));

        }


        [TestMethod]
        /* T06 - This test extracts before pressing a selection button. Nothing should be in the delivery chute
         */
        public void t06_TryToExtractBeforeEnteringCorrectChange() {

            coinKinds = new List<int> { 100, 5, 25, 10 };
            popNames = new List<string> { "Coke", "water", "stuff" };
            popCosts = new List<int> { 250, 250, 205 };

            // Configure the machine
            int vmIndex = vmf.CreateVendingMachine(coinKinds, 3, 2, 10, 10);
            vmf.ConfigureVendingMachine(vmIndex, popNames, popCosts);

            // Load coins in the coin racks
            List<List<Coin>> coinRacks = new List<List<Coin>>();
            coinRacks.Add(new List<Coin>());                                    // Rack 0
            coinRacks.Add(new List<Coin> { new Coin(5) });                      // Rack 1
            coinRacks.Add(new List<Coin> { new Coin(25), new Coin(25) });       // Rack 2
            coinRacks.Add(new List<Coin> { new Coin(10) });                     // Rack 3
            this.LoadCoinRacks(vmIndex, coinRacks);

            // Load pops in the pop racks
            List<List<PopCan>> popRacks = new List<List<PopCan>>();
            popRacks.Add(new List<PopCan> { new PopCan("Coke") });  // Rack 0
            popRacks.Add(new List<PopCan> { new PopCan("water") }); // Rack 1
            popRacks.Add(new List<PopCan> { new PopCan("stuff") }); // Rack 2
            this.LoadPopRacks(vmIndex, popRacks);

            // Insert Coins
            List<int> coinInput = new List<int> { 100, 100, 100 };
            this.insertCoins(vmIndex, coinInput);

            // --- Assert Check Delivery
            // Reality
            delivered = vmf.ExtractFromDeliveryChute(vmIndex);
            // Expected
            expectedChange = 0;
            expectedPops = new List<PopCan>();
            // Assert
            Assert.IsTrue(checkDelivery(expectedChange, expectedPops, delivered));

            // --- Assert Check Teardown
            // Reality
            unloaded = vmf.UnloadVendingMachine(vmIndex);
            // Expected
            expectedCoinsinRack = 65;
            expectedCoinsinStorage = 0;
            expectedPops = new List<PopCan> { new PopCan("Coke"), new PopCan("stuff"), new PopCan("water") };
            // Assert
            Assert.IsTrue(checkTearDown(expectedCoinsinRack, expectedCoinsinStorage, expectedPops, unloaded));

        }

        //--------------------------------------------------------------------------------------------------------------
        
        // Load coins into the rack. Order matters.
        public void LoadCoinRacks(int vmInd, List<List<Coin>> coinRacks) {
            int coinRackIndex = 0;
            foreach (var coinRack in coinRacks) {
                vmf.LoadCoins(vmInd, coinRackIndex, coinRack);
                coinRackIndex++;
            }
        }

        // Loads pops into the rack. Order matters.
        public void LoadPopRacks(int vmInd, List<List<PopCan>> popRacks) {
            int popCanRackIndex = 0;
            foreach (var popRack in popRacks) {
                vmf.LoadPops(vmInd, popCanRackIndex, popRack);
                popCanRackIndex++;
            }
        }

        // Inserts coins by the user.
        public void insertCoins(int vmInd, List<int> coinInput) {
            foreach (var coinin in coinInput) {
                vmf.InsertCoin(vmInd, new Coin(coinin));
            }
        }

        // The methods checkDelivery and CheckTearDown have been taken from SENG 301 Assignment 2 Code written by Tony Tang
        // The specific file is SENG301VMAnalyzer.cs so credit is where credit is due. No plagarism intended. Simply 
        // want the code to be able to check the contents of delivery and teardown properly. 

        // Checks the contest of an extract for delivery
        private bool checkDelivery (int expectedChange, List<PopCan> pops, List<IDeliverable> delivered) {

            var result = true;

            List<IDeliverable> extraction = new List<IDeliverable>();
            extraction.Clear();
            extraction.AddRange(delivered);

            foreach (var item in extraction) {
                if (item is Coin) {
                    expectedChange -= ((Coin)item).Value;
                }
                else if (item is PopCan) {
                    if (pops.Contains((PopCan)item)) {
                        pops.Remove((PopCan)item);
                    }
                    else {
                        result = false;
                        break;
                    }
                }
            }
            if (!((expectedChange == 0) && (pops.Count == 0))) {
                result = false;
            }
            return result;

        }

        // Check the contents of a vending machine teardown
        private bool checkTearDown (int expectedCoinsInRacks, int expectedCoinsInStorage, List<PopCan> pops, VendingMachineStoredContents delivered) {

            var result = true;
            var coinsInCoinRacks = delivered.CoinsInCoinRacks;
            var coinsUsedForPayment = delivered.PaymentCoinsInStorageBin;
            var unsoldPopCanRacks = delivered.PopCansInPopCanRacks;

            foreach (var rack in coinsInCoinRacks) {
                foreach (var coin in rack) {
                    expectedCoinsInRacks -= ((Coin)coin).Value;
                }
            }
            foreach (var coin in coinsUsedForPayment) {
                expectedCoinsInStorage -= ((Coin)coin).Value;
            }
            if (!((expectedCoinsInRacks == 0) && (expectedCoinsInStorage == 0))) {
                result = false;
            }
            else {
                foreach (var popCanRack in unsoldPopCanRacks) {
                    foreach (var popCan in popCanRack) {
                        if (pops.Contains((PopCan)popCan)) {
                            pops.Remove((PopCan)popCan);
                        }
                        else {
                            result = false;
                            break;
                        }
                    }
                }
                if (pops.Count > 0) {
                    result = false;
                }
            }
            return result;
        }

    }

    
    public class VendingMachineFactory : IVendingMachineFactory {

        List<VendingMachine> vendingMachines;

        public VendingMachineFactory() {
            this.vendingMachines = new List<VendingMachine>();
        }

        public int CreateVendingMachine(List<int> coinKinds, int selectionButtonCount, int coinRackCapacity, int popRackCapcity, int receptacleCapacity) {
            var coinKindArray = coinKinds.ToArray();
            var vm = new VendingMachine(coinKindArray, selectionButtonCount, coinRackCapacity, popRackCapcity, receptacleCapacity);
            this.vendingMachines.Add(vm);
            new VendingMachineLogic(vm);
            return this.vendingMachines.Count - 1;
        }

        public void ConfigureVendingMachine(int vmIndex, List<string> popNames, List<int> popCosts) {
            var vm = this.vendingMachines[vmIndex];
            vm.Configure(popNames, popCosts);
        }

        public void LoadCoins(int vmIndex, int coinKindIndex, List<Coin> coins) {
            this.vendingMachines[vmIndex].CoinRacks[coinKindIndex].LoadCoins(coins);
        }

        public void LoadPops(int vmIndex, int popKindIndex, List<PopCan> pops) {
            this.vendingMachines[vmIndex].PopCanRacks[popKindIndex].LoadPops(pops);
        }

        public void InsertCoin(int vmIndex, Coin coin) {
            this.vendingMachines[vmIndex].CoinSlot.AddCoin(coin);
        }

        public void PressButton(int vmIndex, int value) {
            this.vendingMachines[vmIndex].SelectionButtons[value].Press();
        }

        public List<IDeliverable> ExtractFromDeliveryChute(int vmIndex) {
            var vm = this.vendingMachines[vmIndex];
            var items = vm.DeliveryChute.RemoveItems();
            var itemsAsList = new List<IDeliverable>(items);

            return itemsAsList;
        }

        public VendingMachineStoredContents UnloadVendingMachine(int vmIndex) {
            var storedContents = new VendingMachineStoredContents();
            var vm = this.vendingMachines[vmIndex];

            foreach (var coinRack in vm.CoinRacks) {
                storedContents.CoinsInCoinRacks.Add(coinRack.Unload());
            }
            storedContents.PaymentCoinsInStorageBin.AddRange(vm.StorageBin.Unload());
            foreach (var popCanRack in vm.PopCanRacks) {
                storedContents.PopCansInPopCanRacks.Add(popCanRack.Unload());
            }

            return storedContents;
        }
    }

}

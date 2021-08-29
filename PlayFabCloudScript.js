///////////////////////////////////////////////////////////////////////////////////////////////////////
//
// Welcome to your first Cloud Script revision!
//
// Cloud Script runs in the PlayFab cloud and has full access to the PlayFab Game Server API 
// (https://api.playfab.com/Documentation/Server), and it runs in the context of a securely
// authenticated player, so you can use it to implement logic for your game that is safe from
// client-side exploits. 
//
// Cloud Script functions can also make web requests to external HTTP
// endpoints, such as a database or private API for your title, which makes them a flexible
// way to integrate with your existing backend systems.
//
// There are several different options for calling Cloud Script functions:
//
// 1) Your game client calls them directly using the "ExecuteCloudScript" API,
// passing in the function name and arguments in the request and receiving the 
// function return result in the response.
// (https://api.playfab.com/Documentation/Client/method/ExecuteCloudScript)
// 
// 2) You create PlayStream event actions that call them when a particular 
// event occurs, passing in the event and associated player profile data.
// (https://api.playfab.com/playstream/docs)
// 
// 3) For titles using the Photon Add-on (https://playfab.com/marketplace/photon/),
// Photon room events trigger webhooks which call corresponding Cloud Script functions.
// 
// The following examples demonstrate all three options.
//
///////////////////////////////////////////////////////////////////////////////////////////////////////

handlers.OnAcceptTradeRequest = function(args)
{
    var tradeID = args.tradeID;
    var userReadOnlyData = server.GetUserReadOnlyData({ PlayFabId: currentPlayerId });
    var tradeRequestArr = JSON.parse(userReadOnlyData.Data.PlayerReceivedTradeRequests.Value);
    var newTradeRequestArr = [];
    for (let i = 0; i < tradeRequestArr.length; i++)
    {
        if (tradeRequestArr[i].TradeId.Value != tradeID)
        {
            var tradeData = new Object();
            tradeData["PlayerId"] = tradeRequestArr[i].PlayerId.Value;
            tradeData["TradeId"] = tradeRequestArr[i].TradeId.Value;
            newTradeRequestArr.push();
        }
    }
    
    var playerReadonlyData = server.UpdateUserReadOnlyData
    ({
        PlayFabId: currentPlayerId,
        Data:
        {
            "PlayerReceivedTradeRequests": JSON.stringify(newTradeRequestArr)
        }
    });
}

handlers.OnCancelTradeRequest = function(args)
{
    // Remove the trade ID from the other player!
    var friendID = args.friendID;
    var tradeID = args.tradeID;
    var friendReadOnlyData = server.GetUserReadOnlyData({ PlayFabId: friendID });
    var tradeRequestArr = JSON.parse(friendReadOnlyData.Data.PlayerReceivedTradeRequests.Value);
    var newTradeRequestArr = [];
    
    for (let i = 0; i < tradeRequestArr.length; i++)
    {
        if (tradeRequestArr[i].TradeId.Value != tradeID)
        {
            var tradeData = new Object();
            tradeData["PlayerId"] = tradeRequestArr[i].PlayerId.Value;
            tradeData["TradeId"] = tradeRequestArr[i].TradeId.Value;
            newTradeRequestArr.push();
        }
    }
    
    var playerReadonlyData = server.UpdateUserReadOnlyData
    ({
        PlayFabId: friendID,
        Data:
        {
            "PlayerReceivedTradeRequests": JSON.stringify(newTradeRequestArr)
        }
    });
}

handlers.OnSendTradeRequest = function(args)
{
    var obj = new Object();
    obj.code = "ERROR";
    
    var friendID = args.friendID;
    var tradeID = args.tradeID;
    var friendReadOnlyData = server.GetUserReadOnlyData({ PlayFabId: friendID });
    
    var tradeRequestArr = JSON.parse(friendReadOnlyData.Data.PlayerReceivedTradeRequests.Value);
    log.info(tradeRequestArr);
    
    if (tradeRequestArr.find(str => str == tradeID) != null)
    {
        log.info("Duplicate trade ID");
        return JSON.stringify(obj);
    }
    
    var tradeData = new Object();
    
    tradeData["PlayerId"] = currentPlayerId;
    tradeData["TradeId"] = tradeID;
    
    tradeRequestArr.push(tradeData);
    
    log.info(JSON.stringify(tradeRequestArr));
    var playerReadonlyData = server.UpdateUserReadOnlyData
    ({
        PlayFabId: friendID,
        Data:
        {
            "PlayerReceivedTradeRequests": JSON.stringify(tradeRequestArr)
        }
    });
    obj.code = "SUCCESS";
    return obj;
    // Because the receiver does not know what trade requests he has received, he must have it stored inside of a JSON array in user data
    // Follows the following format:
    // Initiator ID
    // Trade ID
}

handlers.GetRNCFinal = function(args)
{
            var RNGMod = 10;
            var RNGModFinal = Math.min(10, 50);
            var expeditionTimeMod = parseInt(args.Duration * 2) / 60000;
            log.info(expeditionTimeMod);
            expeditionTimeMod = (Math.random() * expeditionTimeMod) + expeditionTimeMod;
            
            var RNCurrencyGain = Math.floor(((expeditionTimeMod * expeditionTimeMod) * ((RNGMod / (60.0 - RNGModFinal)) + Math.log(RNGMod * 0.5))));
            log.info("Done calculating Currency Gain: " + RNCurrencyGain);
}

handlers.test = function(args, context)
{ 
    var userReadOnlyData = server.GetUserReadOnlyData({ PlayFabId: currentPlayerId });
    log.info(userReadOnlyData.Data["PlayerBaseRNGMod"].Value);
}

handlers.AddTradeList = function(args)
{
    var userReadOnlyData = server.GetUserReadOnlyData({ PlayFabId: currentPlayerId });
    
    if (userReadOnlyData["PlayerReceivedTradeRequests"] != null)
    {
        return;
    }
    
    var playerReadonlyData = server.UpdateUserReadOnlyData
    ({
        PlayFabId: currentPlayerId,
        Data:
        {
            "PlayerReceivedTradeRequests": JSON.stringify([])
        }
    });
}

handlers.GetOthersInventory = function (args)
{
    var inventory = server.GetUserInventory({ PlayFabId: args.ID });
    var returnResult = {};
    
    for (var i = 0; i < inventory.Inventory.length; i++) 
    {
        returnResult[inventory.Inventory[i].ItemInstanceId] = inventory.Inventory[i];
    }
    
    // Good morning why is playfab such a pain in the ass
    var returnResultString = JSON.stringify(returnResult);
    log.info(returnResultString);
    return returnResultString;
}

handlers.OnPlayerConsumedItem = function(args, context)
{   
    var catalog = server.GetCatalogItems({CatalogVersion: "1.00"});
    var userReadOnlyData = server.GetUserReadOnlyData({ PlayFabId: currentPlayerId });
    var consumableID = context.playStreamEvent.ItemId;
    log.info("consumableID: " + consumableID);
    
    var catalogConsumable = catalog.Catalog.find(catalogItem => catalogItem.ItemId == consumableID);
    if (catalogConsumable == null)
    {
        return;
    }
    
    var consumableClass = catalogConsumable.ItemClass;
    
    if (consumableClass == "Potion")
    {
        var consumableCustomData = JSON.parse(catalog.Catalog.find(catalogItem => catalogItem.ItemId == consumableID).CustomData);
        // Workaround...
        var keyString = consumableCustomData.AffectedAttribute;
        var dataPayload = {};
        dataPayload[keyString] = parseInt(consumableCustomData.AddedValue) + parseInt(userReadOnlyData.Data[keyString].Value);
        
        log.info(keyString);
        log.info(consumableCustomData.AddedValue);
        log.info(userReadOnlyData.Data[keyString].Value);
        
        log.info(parseInt(consumableCustomData.AddedValue));
        log.info(parseInt(userReadOnlyData.Data[keyString].Value));

        log.info(dataPayload[keyString]);
        
        var playerReadonlyData = server.UpdateUserReadOnlyData
        ({
            PlayFabId: currentPlayerId,
            Data: dataPayload
        });
    }
    else if (consumableClass == "Economy")
    {
        var consumableCustomData = JSON.parse(catalog.Catalog.find(catalogItem => catalogItem.ItemId == consumableID).CustomData);
        // Workaround...
        var moneyGiven = parseInt(consumableCustomData.MoneyValue);
        server.AddUserVirtualCurrency({ PlayFabID: currentPlayerId, VirtualCurrency: "RN", Amount: moneyGiven });
    }
    
}

handlers.EquipItem = function(args)
{
    var obj = new Object();
    obj.code = "ERROR";
    
    log.info(args);
    var equipSlot = args.EquipSlot;
    var itemInstanceID = args.ItemInstanceID;
    
    log.info("args equipslot: " + equipSlot);
    log.info("args itemID: " + itemInstanceID);
    
    var playerInventory = server.GetUserInventory( { PlayFabId: currentPlayerId } );
    var userReadOnlyData = server.GetUserReadOnlyData({ PlayFabId: currentPlayerId });
    var catalog = server.GetCatalogItems({CatalogVersion: "1.00"});
    
    // Checks if the item exists in the player Inventory, then returns it if it does
    var inventoryItem = playerInventory.Inventory.find(item => item.ItemInstanceId == itemInstanceID);
    log.info("Inventory Item: " + inventoryItem);
    
    // Checks if the catalog contains the item the inventory item ID represents, then tries to return the equip slot OFF the catalog
    var inventoryItemEquipSlot = JSON.parse(catalog.Catalog.find(catalogItem => catalogItem.ItemId == inventoryItem.ItemId).CustomData).EquipSlot;
    log.info("Catalog equip slot: " + inventoryItemEquipSlot);
    log.info(inventoryItem != null && inventoryItemEquipSlot != null);
    
    // Workaround...
    var keyString = inventoryItemEquipSlot;
    var dataPayload = {};
    dataPayload[keyString] = itemInstanceID;
    
    if (inventoryItem != null && inventoryItemEquipSlot != null)
    {
        log.info("Updating Equipment...");
        var playerReadonlyData = server.UpdateUserReadOnlyData
        ({
            PlayFabId: currentPlayerId,
            Data: dataPayload
        });
    }
}

handlers.GetExpeditionRewards = function(args)
{
    var obj = new Object();
    obj.code = "ERROR";
    obj.RNCurrencyGain = 0;
    obj.isTreasureGained = false;
    obj.treasureName = "";
    
    // Get all necessary data
    var userReadOnlyData = server.GetUserReadOnlyData({ 
        PlayFabId: currentPlayerId
    });
    var playerInventory = server.GetUserInventory( { PlayFabId: currentPlayerId } );
    var catalog = server.GetCatalogItems({CatalogVersion: "1.00"});
    
    
    var timeSinceStart = Date.now() - userReadOnlyData.Data.PlayerAdventureStartDate.Value;
    var timeToFinish = userReadOnlyData.Data.PlayerAdventureEndDate.Value - userReadOnlyData.Data.PlayerAdventureStartDate.Value;
    log.info("time since start: " + timeSinceStart);
    log.info("time to finish: " + timeToFinish);
    
    log.info("Checking if the player is on an adventure");
    if (userReadOnlyData.Data.IsPlayerAdventuring.Value != "false")
    {
        log.info("Checking if the can collect rewards");
        if (timeSinceStart > timeToFinish)
        {
            // Get some variables to be used in calculation
            var playerRNGBaseMod = parseInt(playerRNGBaseMod = userReadOnlyData.Data.PlayerBaseRNGMod.Value);
            var playerBaseTreasureMod = parseInt(playerBaseTreasureMod = userReadOnlyData.Data.PlayerBaseTreasureMod.Value);
            var equippedRNGSword = playerInventory.Inventory.find(item => item.ItemInstanceId == userReadOnlyData.Data.EquippedSword.Value);
            var equippedTreasureFinder = playerInventory.Inventory.find(item => item.ItemInstanceId == userReadOnlyData.Data.EquippedTreasureFinder.Value);
            
            if (equippedTreasureFinder == null || equippedRNGSword == null)
            {
                return JSON.stringify(obj);
            }
            
            var swordMod = parseInt(JSON.parse(catalog.Catalog.find(catalogItem => catalogItem.ItemId == equippedRNGSword.ItemId).CustomData).RNGMod);
            var treasureFinderMod = parseInt(JSON.parse(catalog.Catalog.find(catalogItem => catalogItem.ItemId == equippedTreasureFinder.ItemId).CustomData).TreasureMod);
            
            var TotalExpeditionTime = userReadOnlyData.Data.PlayerExpeditionActualDuration.Value / 60000;
            log.info("Total Expedition Time: " + TotalExpeditionTime);
            
            log.info("Player base RNG: " + playerRNGBaseMod);
            log.info("Weapon RNG:  " + swordMod);
            
            log.info("Player base Treasure chance: " + playerBaseTreasureMod);
            log.info("Treasure finder chance: " + treasureFinderMod);
            
            log.info("Done calculating variables");
            
            // Calculate RNCurrency Gain
            var RNGMod = playerRNGBaseMod + swordMod;
            var RNGModFinal = Math.min(playerRNGBaseMod + swordMod, 50);
            var expeditionTimeMod = TotalExpeditionTime * 2;
            expeditionTimeMod = (Math.random() * expeditionTimeMod) + expeditionTimeMod;
            
            var RNCurrencyGain = Math.floor(((expeditionTimeMod * expeditionTimeMod) * ((RNGMod / (60.0 - RNGModFinal)) + Math.log(RNGMod * 0.5))));
            obj.RNCurrencyGain = RNCurrencyGain;
            log.info("Done calculating Currency Gain: " + RNCurrencyGain);
            
            // Check if treasure is found
            var treasureChance = Math.floor(Math.random() * 100);
            log.info("Calculated Chance: " + treasureChance);
            log.info ("Must roll below: " + (playerBaseTreasureMod + treasureFinderMod));
            
            // If treasure found, see which one the player gets
            if (treasureChance < (playerBaseTreasureMod + treasureFinderMod))
            {
                var lesserChance = Math.floor(Math.pow(1.005, (TotalExpeditionTime)) * 18.0)
                var normalChance = Math.floor(Math.pow(1.0065, (TotalExpeditionTime)) * 3.8);
                var rareChance = Math.floor(Math.pow(1.008, (TotalExpeditionTime)) * 0.2)
                var actualChance = Math.random() * (lesserChance + normalChance + rareChance);
                let itemChanceArray = [rareChance, normalChance + rareChance, lesserChance + normalChance + rareChance];
                
                log.info("Treasure found");
                log.info("lesserChance: " + lesserChance);
                log.info("normalChance: " + normalChance);
                log.info("rareChance: " + rareChance);
                log.info("actual chance: " + actualChance);
                // Fuck Enums
                var treasureType = 0 
                for (let i =  0; i < 3; i++)
                {
                    // THIS CODES SUCK SO MUCH BUT I FUCKING LAZY ALR LOL
                    var temp = Math.min(actualChance, itemChanceArray[i]);
                    if (temp == actualChance)
                    {
                        treasureType = i;
                        break;
                    }
                }
                
                // Determine the item type
                var itemType =  Math.floor(Math.random() * 3);
                
                let itemArray = ["RNGSword", "TimeAmulet", "TreasureFinder"];
                let modArray = ["Legendary", "Mastercraft", "Lesser"]
                
                var finalItemID = modArray[treasureType] + itemArray[itemType];
                
                obj.isTreasureGained = true;
                obj.treasureName = finalItemID;
                
                log.info(finalItemID);
                
                var itemGrantResult = server.GrantItemsToUser
                ({
                    PlayFabId: currentPlayerId,
                    ItemIDs: finalItemID
                });
            }
            
            
            obj.code = "EXPEDITION_DONE";
            
            // Mark player as done expeditioning, and give him his rewards
            var playerReadonlyDataUpdate = server.UpdateUserReadOnlyData
            ({
                PlayFabId: currentPlayerId,
                Data:
                {
                    "IsPlayerAdventuring" : false,
                }
            });
            
            server.AddUserVirtualCurrency({ PlayFabID: currentPlayerId, VirtualCurrency: "RN", Amount: obj.RNCurrencyGain });
            
            var playerInventoryUpdate = server.UpdatePlay
       }
       else
       {
           log.info("Not ready to collect rewards");
           log.info("Seconds remaining: " + (timeToFinish - timeSinceStart));
       }
   }
   var stringObj = JSON.stringify(obj);
   return stringObj;
}

handlers.SendOnExpedition = function(args)
{
    var duration = parseInt(args.Duration);
    
    var obj = new Object();
    obj.code = "ERROR";
    obj.startTime = 0;
    obj.endTime = 0;
    
    var userReadOnlyData = server.GetUserReadOnlyData({ PlayFabId: currentPlayerId });
    var playerInventory = server.GetUserInventory( { PlayFabId: currentPlayerId } );
    var catalog = server.GetCatalogItems({CatalogVersion: "1.00"});
     
    log.info("Is on expedition? " + userReadOnlyData.Data.IsPlayerAdventuring.Value);
    log.info("Duration sent: " + duration);
    log.info(userReadOnlyData.Data.IsPlayerAdventuring.Value == "false");
    log.info(duration >= 60000);
    log.info(duration <= 90000000);

    // Must be more than 1 min (60000ms)
    if (userReadOnlyData.Data.IsPlayerAdventuring.Value == "false" && duration >= 60000 && duration <= 90000000)
    {
        log.info("Sending user on expedition, calculating variables...");
        
        log.info("Player amulet ID: " + userReadOnlyData.Data.EquippedAmulet.Value);

        var equippedAmulet = playerInventory.Inventory.find(item => item.ItemInstanceId == userReadOnlyData.Data.EquippedAmulet.Value);
        
        if (equippedAmulet == null)
        {
            return JSON.stringify(obj);
        }
        
        let amuletMod = parseInt(JSON.parse(catalog.Catalog.find(catalogItem => catalogItem.ItemId == equippedAmulet.ItemId).CustomData).TimeMod);
        let playerMod = parseInt(userReadOnlyData.Data.PlayerBaseTimeMod.Value);
        
        log.info("Player mod: ", amuletMod);
        log.info("Amulet mod: ", playerMod);
        
        var newDuration = 0;
        log.info("Is total mod > 100? ", (playerMod + amuletMod) >= 100);
        if ((playerMod + amuletMod) >= 100)
        {
            newDuration = 60000;
            adventureDate = Date.now();
        }
        else
        {
            log.info("New Duration", newDuration);
            log.info("Percentage", (1.0 - ((playerMod + amuletMod) / 100.0)));
            newDuration = Math.floor(duration * (1.0 - ((playerMod + amuletMod) / 100.0)));
            adventureDate = Date.now();
        }
        
        log.info("Is new duration mod < 60000? ", newDuration <= 60000);
        if (newDuration <= 60000)
        {
            newDuration = 60000;
        }
        
        log.info("Initial duration: ", duration)
        log.info("Final duration: ", newDuration)
        
        var endDate = new Date(Date.now() + newDuration);
        var adventureDate = new Date(Date.now());
        
        log.info("start time: ", (adventureDate - new Date(0)));
        log.info("end time: ", (endDate - new Date(0)));
        
        
        endDate.setMilliseconds(0);
        
        var playerReadonlyData = server.UpdateUserReadOnlyData
        ({
            PlayFabId: currentPlayerId,
            Data:
            {
                "IsPlayerAdventuring" : true,
                "PlayerAdventureStartDate": (adventureDate - new Date(0)),
                "PlayerAdventureEndDate": (endDate - new Date(0)),
                "PlayerExpeditionActualDuration" : duration
            }
        });
        
        obj.code = "EXPEDITION_STARTED";
        obj.startTime = adventureDate;
        obj.endTime = adventureDate + newDuration;
    }
    
    return JSON.stringify(obj);
}

handlers.GiveStarterInventory = function(args)
{
    var userReadOnlyData = server.GetUserReadOnlyData({ PlayFabId: currentPlayerId });
    
    if (userReadOnlyData.PlayerIsSetup != null)
    {
        log.info("Client tried to give itself starter items.");
        return;
    }
    
    if (userReadOnlyData.Data.PlayerIsSetup == "true")
    {
        log.info("Client tried to give itself starter items.");
        return;
    }
    
    var itemGrantResult = server.GrantItemsToUser
    ({
        PlayFabId: currentPlayerId,
        ItemIDs: ["PracticeRNGSword", "PracticeTimeAmulet", "PracticeTreasureFinder"]
    });
    
    var userInventoryData = server.GetUserInventory
    ({ 
        PlayFabId: currentPlayerId
    });
    
    var creationTime = Date.now();
	
    server.UpdateUserReadOnlyData
    ({
        PlayFabId: currentPlayerId,
        Data:
        {
            "EquippedAmulet": userInventoryData.Inventory.find(item => item.ItemId == "PracticeTimeAmulet").ItemInstanceId,
            "EquippedSword": userInventoryData.Inventory.find(item => item.ItemId == "PracticeRNGSword").ItemInstanceId,
            "EquippedTreasureFinder": userInventoryData.Inventory.find(item => item.ItemId == "PracticeTreasureFinder").ItemInstanceId,
            "IsPlayerAdventuring" : false,
            "PlayerAdventureStartDate": creationTime,
            "PlayerAdventureEndDate": creationTime,
            "PlayerIsSetup": true,
            "PlayerReceivedTradeRequests": JSON.stringify([])
        }
    });
    
    server.UpdateUserReadOnlyData
    ({
        PlayFabId: currentPlayerId,
        Data:
        {
            "PlayerBaseRNGMod": 5,
            "PlayerBaseTreasureMod": 1,
            "PlayerBaseTimeMod": 0,
        }
    });
}


// This is a Cloud Script function. "args" is set to the value of the "FunctionParameter" 
// parameter of the ExecuteCloudScript API.
// (https://api.playfab.com/Documentation/Client/method/ExecuteCloudScript)
// "context" contains additional information when the Cloud Script function is called from a PlayStream action.
handlers.helloWorld = function (args, context) {
    
    // The pre-defined "currentPlayerId" variable is initialized to the PlayFab ID of the player logged-in on the game client. 
    // Cloud Script handles authenticating the player automatically.
    var message = "Hello " + currentPlayerId + "!";

    // You can use the "log" object to write out debugging statements. It has
    // three functions corresponding to logging level: debug, info, and error. These functions
    // take a message string and an optional object.
    log.info(message);
    var inputValue = null;
    if (args && args.inputValue)
        inputValue = args.inputValue;
    log.debug("helloWorld:", { input: args.inputValue });

    // The value you return from a Cloud Script function is passed back 
    // to the game client in the ExecuteCloudScript API response, along with any log statements
    // and additional diagnostic information, such as any errors returned by API calls or external HTTP
    // requests. They are also included in the optional player_executed_cloudscript PlayStream event 
    // generated by the function execution.
    // (https://api.playfab.com/playstream/docs/PlayStreamEventModels/player/player_executed_cloudscript)
    return { messageValue: message };
};

// This is a simple example of making a PlayFab server API call
handlers.makeAPICall = function (args, context) {
    var request = {
        PlayFabId: currentPlayerId, Statistics: [{
                StatisticName: "Level",
                Value: 2
            }]
    };
    // The pre-defined "server" object has functions corresponding to each PlayFab server API 
    // (https://api.playfab.com/Documentation/Server). It is automatically 
    // authenticated as your title and handles all communication with 
    // the PlayFab API, so you don't have to write extra code to issue HTTP requests. 
    var playerStatResult = server.UpdatePlayerStatistics(request);
};

// This an example of a function that calls a PlayFab Entity API. The function is called using the 
// 'ExecuteEntityCloudScript' API (https://api.playfab.com/documentation/CloudScript/method/ExecuteEntityCloudScript).
handlers.makeEntityAPICall = function (args, context) {

    // The profile of the entity specified in the 'ExecuteEntityCloudScript' request.
    // Defaults to the authenticated entity in the X-EntityToken header.
    var entityProfile = context.currentEntity;

    // The pre-defined 'entity' object has functions corresponding to each PlayFab Entity API,
    // including 'SetObjects' (https://api.playfab.com/documentation/Data/method/SetObjects).
    var apiResult = entity.SetObjects({
        Entity: entityProfile.Entity,
        Objects: [
            {
                ObjectName: "obj1",
                DataObject: {
                    foo: "some server computed value",
                    prop1: args.prop1
                }
            }
        ]
    });

    return {
        profile: entityProfile,
        setResult: apiResult.SetResults[0].SetResult
    };
};

// This is a simple example of making a web request to an external HTTP API.
handlers.makeHTTPRequest = function (args, context) {
    var headers = {
        "X-MyCustomHeader": "Some Value"
    };
    
    var body = {
        input: args,
        userId: currentPlayerId,
        mode: "foobar"
    };

    var url = "http://httpbin.org/status/200";
    var content = JSON.stringify(body);
    var httpMethod = "post";
    var contentType = "application/json";

    // The pre-defined http object makes synchronous HTTP requests
    var response = http.request(url, httpMethod, content, contentType, headers);
    return { responseContent: response };
};

// This is a simple example of a function that is called from a
// PlayStream event action. (https://playfab.com/introducing-playstream/)
handlers.handlePlayStreamEventAndProfile = function (args, context) {
    
    // The event that triggered the action 
    // (https://api.playfab.com/playstream/docs/PlayStreamEventModels)
    var psEvent = context.playStreamEvent;
    
    // The profile data of the player associated with the event
    // (https://api.playfab.com/playstream/docs/PlayStreamProfileModels)
    var profile = context.playerProfile;
    
    // Post data about the event to an external API
    var content = JSON.stringify({ user: profile.PlayerId, event: psEvent.EventName });
    var response = http.request('https://httpbin.org/status/200', 'post', content, 'application/json', null);

    return { externalAPIResponse: response };
};


// Below are some examples of using Cloud Script in slightly more realistic scenarios

// This is a function that the game client would call whenever a player completes
// a level. It updates a setting in the player's data that only game server
// code can write - it is read-only on the client - and it updates a player
// statistic that can be used for leaderboards. 
//
// A funtion like this could be extended to perform validation on the 
// level completion data to detect cheating. It could also do things like 
// award the player items from the game catalog based on their performance.
handlers.completedLevel = function (args, context) {
    var level = args.levelName;
    var monstersKilled = args.monstersKilled;
    
    var updateUserDataResult = server.UpdateUserInternalData({
        PlayFabId: currentPlayerId,
        Data: {
            lastLevelCompleted: level
        }
    });

    log.debug("Set lastLevelCompleted for player " + currentPlayerId + " to " + level);
    var request = {
        PlayFabId: currentPlayerId, Statistics: [{
                StatisticName: "level_monster_kills",
                Value: monstersKilled
            }]
    };
    server.UpdatePlayerStatistics(request);
    log.debug("Updated level_monster_kills stat for player " + currentPlayerId + " to " + monstersKilled);
};


// In addition to the Cloud Script handlers, you can define your own functions and call them from your handlers. 
// This makes it possible to share code between multiple handlers and to improve code organization.
handlers.updatePlayerMove = function (args) {
    var validMove = processPlayerMove(args);
    return { validMove: validMove };
};


// This is a helper function that verifies that the player's move wasn't made
// too quickly following their previous move, according to the rules of the game.
// If the move is valid, then it updates the player's statistics and profile data.
// This function is called from the "UpdatePlayerMove" handler above and also is 
// triggered by the "RoomEventRaised" Photon room event in the Webhook handler
// below. 
//
// For this example, the script defines the cooldown period (playerMoveCooldownInSeconds)
// as 15 seconds. A recommended approach for values like this would be to create them in Title
// Data, so that they can be queries in the script with a call to GetTitleData
// (https://api.playfab.com/Documentation/Server/method/GetTitleData). This would allow you to
// make adjustments to these values over time, without having to edit, test, and roll out an
// updated script.
function processPlayerMove(playerMove) {
    var now = Date.now();
    var playerMoveCooldownInSeconds = 15;

    var playerData = server.GetUserInternalData({
        PlayFabId: currentPlayerId,
        Keys: ["last_move_timestamp"]
    });

    var lastMoveTimestampSetting = playerData.Data["last_move_timestamp"];

    if (lastMoveTimestampSetting) {
        var lastMoveTime = Date.parse(lastMoveTimestampSetting.Value);
        var timeSinceLastMoveInSeconds = (now - lastMoveTime) / 1000;
        log.debug("lastMoveTime: " + lastMoveTime + " now: " + now + " timeSinceLastMoveInSeconds: " + timeSinceLastMoveInSeconds);

        if (timeSinceLastMoveInSeconds < playerMoveCooldownInSeconds) {
            log.error("Invalid move - time since last move: " + timeSinceLastMoveInSeconds + "s less than minimum of " + playerMoveCooldownInSeconds + "s.");
            return false;
        }
    }

    var playerStats = server.GetPlayerStatistics({
        PlayFabId: currentPlayerId
    }).Statistics;
    var movesMade = 0;
    for (var i = 0; i < playerStats.length; i++)
        if (playerStats[i].StatisticName === "")
            movesMade = playerStats[i].Value;
    movesMade += 1;
    var request = {
        PlayFabId: currentPlayerId, Statistics: [{
                StatisticName: "movesMade",
                Value: movesMade
            }]
    };
    server.UpdatePlayerStatistics(request);
    server.UpdateUserInternalData({
        PlayFabId: currentPlayerId,
        Data: {
            last_move_timestamp: new Date(now).toUTCString(),
            last_move: JSON.stringify(playerMove)
        }
    });

    return true;
}

// This is an example of using PlayStream real-time segmentation to trigger
// game logic based on player behavior. (https://playfab.com/introducing-playstream/)
// The function is called when a player_statistic_changed PlayStream event causes a player 
// to enter a segment defined for high skill players. It sets a key value in
// the player's internal data which unlocks some new content for the player.
handlers.unlockHighSkillContent = function (args, context) {
    var playerStatUpdatedEvent = context.playStreamEvent;
    var request = {
        PlayFabId: currentPlayerId,
        Data: {
            "HighSkillContent": "true",
            "XPAtHighSkillUnlock": playerStatUpdatedEvent.StatisticValue.toString()
        }
    };
    var playerInternalData = server.UpdateUserInternalData(request);
    log.info('Unlocked HighSkillContent for ' + context.playerProfile.DisplayName);
    return { profile: context.playerProfile };
};

// Photon Webhooks Integration
//
// The following functions are examples of Photon Cloud Webhook handlers. 
// When you enable the Photon Add-on (https://playfab.com/marketplace/photon/)
// in the Game Manager, your Photon applications are automatically configured
// to authenticate players using their PlayFab accounts and to fire events that 
// trigger your Cloud Script Webhook handlers, if defined. 
// This makes it easier than ever to incorporate multiplayer server logic into your game.


// Triggered automatically when a Photon room is first created
handlers.RoomCreated = function (args) {
    log.debug("Room Created - Game: " + args.GameId + " MaxPlayers: " + args.CreateOptions.MaxPlayers);
};

// Triggered automatically when a player joins a Photon room
handlers.RoomJoined = function (args) {
    log.debug("Room Joined - Game: " + args.GameId + " PlayFabId: " + args.UserId);
};

// Triggered automatically when a player leaves a Photon room
handlers.RoomLeft = function (args) {
    log.debug("Room Left - Game: " + args.GameId + " PlayFabId: " + args.UserId);
};

// Triggered automatically when a Photon room closes
// Note: currentPlayerId is undefined in this function
handlers.RoomClosed = function (args) {
    log.debug("Room Closed - Game: " + args.GameId);
};

// Triggered automatically when a Photon room game property is updated.
// Note: currentPlayerId is undefined in this function
handlers.RoomPropertyUpdated = function (args) {
    log.debug("Room Property Updated - Game: " + args.GameId);
};

// Triggered by calling "OpRaiseEvent" on the Photon client. The "args.Data" property is 
// set to the value of the "customEventContent" HashTable parameter, so you can use
// it to pass in arbitrary data.
handlers.RoomEventRaised = function (args) {
    var eventData = args.Data;
    log.debug("Event Raised - Game: " + args.GameId + " Event Type: " + eventData.eventType);

    switch (eventData.eventType) {
        case "playerMove":
            processPlayerMove(eventData);
            break;

        default:
            break;
    }
};

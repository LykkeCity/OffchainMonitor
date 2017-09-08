# Offchain Monitor

[![Build status](https://teamcity.lykkex.net/app/rest/builds/buildType:(id:OffchainMonitor_BUILD)/statusIcon)](http://teamcity.lykkex.net/viewType.html?buildTypeId=OffchainMonitor_BUILD&guest=1)

## Summary

This is the monitor application (in the form of asp.net application), for monitoring Bitcoin blockchain for commitments which should not be broadcasted. If they are found broadcasted, the respective punishment will be broadcasted.

This app is designed to be hosted using Docker.

## Initialization

*   The multisig address to be monitored should be specified using /api/Settings/SetMultisig endpoint.

## Api

Api is accessable at /swagger/v1/swagger.json endpoint

## Related material

*   The offchain material on LykkeCity gihub, including bitcoinservice repository or the the old Offchain repository.

*   lightning.network related material for example: https://lightning.network/lightning-network-paper.pdf

## Configuration

Currently the OffchainMonitorRunner reads its setting through an environment variable called OffchainMonitorSettings. This is a json string like {  "NetworkType": 1,  "RPCUsername": "xxx",  "RPCPassword": "xxx",  "RPCServerIpAddress": "xxx",  "QBitNinjaBaseUrl": "xxx"} which should be configured according to the environment. (0 is for mainnet and 1 is for testnet).

The configuration for bitcoin daemon should be like following:

```
regtest=1
prematurewitness=1
server=1
listen=1
port=18333
rpcallowip=0.0.0.0/0
rpcport=18332
rpcuser=xxx
rpcpassword=xxx
datadir=D:\Bitcoin\datadir
txindex=1
```

## Testing

BlockchainStateManager is used to put a bitcoin daemon of regtest mode in a known state, so things could be tested.

### Required software for testing

Bitcoin daemon in regtest mode (Supporting SegWit, version > 0.13) , Azure storage emulator, QBitNinja, QBitNinja.Listener.Console, Sql Server (Express edition would work), colorcore ( https://github.com/OpenAssets/colorcore )

*   Azure storage emulator, QBitNinja, colorcore should be be running before test running
*   The BlockchainStateManager should be run in administrator mode since there is a management code for IIS (which stops/starts iis service, and is available for windows only). This includes visual studio if testing is being done through it.

### Testing Procedure

In order to run the application, besides starting in in the Administrator mode, following should also be configured:
*   QBitNinja.Listener.Console: To index newly generated blocks and transactions
*   QBitNinja: To be able to query blockchain data through its API
*   Azure Storage Emulator: To enable local storage for QBitNinja
*   Colorcore (https://github.com/OpenAssets/colorcore): To enable issuing of assets and transfering them (Currently the development is for bitcoin itself)
*   It is needed to adapt App.config to the running enironment

After the BlockchainStateManager run finished, a file named log.txt will be created. (It will probably take some minutes, sometimes, it may terminate in between which needs to close bitcoin daemon and QBitNinja.Listener.Console and run it again, it is not stable enough).
The log.txt will contain 3 transactions:
*   The unsigned commitment which is signed by client and given to Lykke. If Lykke revokes this transaction by giving the private key to client, the client can create the respective punishment and submit the commitment and punishment to Lykke. This commitment will not have Lykke signature on it because client does not have access to Lykke private key, but since it is a segwit transaction it could be monitored by using its tx hash which is independent of signatures.
*   Punishment transaction, which is used to be submitted alongside the above parameter to monitoring service.
*   The signed commitment transaction, which is used to be submitted to blockchain using bitcoin-cli command for example.

After above 3 transactions were obtained the monitor service itself could be executed (for example using "dotnet OffchainMonitorRunner.dll"). At this stage the signed version could be broadcasted to network using bitcoin-cli command line, and the monitor should detect it and broadcast the punishment.

## Docker

### Requirements

It is required to have access to A- a QBit.Ninja instance (for exapmle http://api.qbit.ninja/ to explore blockchain) and B- The RPC endpoint of a Bitcoin Daemon instance (to broadcast the punishment).

### Running

To run the docker image, a command like following could be adopted and executed:

```
docker run -e OffchainMonitorSettings="{  'NetworkType': 0,  'RPCUsername':'{BitcoinDaemonUsername}', 'RPCPassword': '{BitcoinDaemonPassword}',  'RPCServerIpAddress': '{BitcoinDaemonIp}',  'QBitNinjaBaseUrl': '{QBitNinjaAddress}'}" -p {HostBinding}:5000 {DockerImageName} port=5000
```

An example for test of above is:

```
docker run -e OffchainMonitorSettings="{  'NetworkType': 1,  'RPCUsername':'xxx', 'RPCPassword': 'xxx',  'RPCServerIpAddress': '192.168.0.125',  'QBitNinjaBaseUrl': 'http://192.168.0.125:85/'}" -p 127.0.0.1:502:5000 offchainmonitorrunner:latest port=5000
```

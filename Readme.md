# Offchain Monitor

[![Build status](https://ci.appveyor.com/api/projects/status/jjncv8d8i8482q68?svg=true)](https://ci.appveyor.com/project/lykke/offchainmonitor)

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

## Testing

BlockchainStateManager is used to put a bitcoin daemon of regtest mode in a known state, so things could be tested.
Running BlockchainStateManager should be practiced in Administrator mode (it is available for windows only), since it currently stops/starts iis service.

### Required software for testing

Bitcoin daemon in regtest mode (Supporting SegWit, version > 0.13) , Azure storage emulator, QBitNinja, QBitNinja.Listener.Console, Sql Server (Express edition would work), colorcore ( https://github.com/OpenAssets/colorcore )

*   Azure storage emulator, QBitNinja, colorcore should be be running before test running
*   The BlockchainStateManager should be run in administrator mode since there is a management code for IIS. This includes visual studio if testing is being done through it.



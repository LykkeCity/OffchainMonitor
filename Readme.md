# Offchain Monitor

[![Build status](https://ci.appveyor.com/api/projects/status/jjncv8d8i8482q68?svg=true)](https://ci.appveyor.com/project/lykke/offchainmonitor)

## Summary

This the monitor application (in the form of asp.net application), for monitoring Bitcoin blockchain for commitments which should not be broadcasted. If they are found broadcasted, the respective punishment will be broadcasted.

This app is designed to be hosted using Docker.

## Iniitialization

*   The multisig address to be monitored should be specified using /api/Settings/SetMultisig endpoint.

## Api

Api is accessable at /swagger/v1/swagger.json endpoint

## Related material

*   The offchain material on LykkeCity gihub, including bitcoinservice repository or the the old Offchain repository.

*   lightning.network related material for example: https://lightning.network/lightning-network-paper.pdf
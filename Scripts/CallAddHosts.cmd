@echo off
ECHO Running CallAddHosts

set hosts="accounts.dispro.network.local api.dispro.network.local dispro.network.local"
CD .\Scripts
.\AddHosts.cmd %hosts%
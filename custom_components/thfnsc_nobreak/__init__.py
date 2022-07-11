"""The Nobreak integration."""
from __future__ import annotations

from datetime import timedelta
import logging

import aiohttp
import async_timeout

from homeassistant.config_entries import ConfigEntry
from homeassistant.const import Platform
from homeassistant.core import HomeAssistant
from homeassistant.helpers.update_coordinator import DataUpdateCoordinator

from .const import (
    CONF_ENDPOINT,
    CONF_SKIP_SSL_VALIDATION,
    DATA_API,
    DATA_COORDINATOR,
    DOMAIN,
)

PLATFORMS: list[Platform] = [Platform.SENSOR, Platform.SWITCH]

_LOGGER = logging.getLogger(__name__)


async def async_setup_entry(hass: HomeAssistant, entry: ConfigEntry) -> bool:
    """Set up Nobreak from a config entry."""

    api = NobreakApi(entry.data[CONF_ENDPOINT], entry.data[CONF_SKIP_SSL_VALIDATION])
    hass.data[DOMAIN] = {}
    hass.data[DOMAIN][DATA_API] = api
    coordinator = NobreakCoordinator(hass, api)
    hass.data[DOMAIN][DATA_COORDINATOR] = coordinator
    await coordinator.async_config_entry_first_refresh()

    hass.config_entries.async_setup_platforms(entry, PLATFORMS)

    return True


async def async_unload_entry(hass: HomeAssistant, entry: ConfigEntry) -> bool:
    """Unload a config entry."""
    if unload_ok := await hass.config_entries.async_unload_platforms(entry, PLATFORMS):
        hass.data[DOMAIN].pop(DATA_API)

    return unload_ok


class NobreakCoordinator(DataUpdateCoordinator):
    """Nobreak coordinator."""

    def __init__(self, hass, api: NobreakApi):
        """Initialize my coordinator."""
        super().__init__(
            hass,
            _LOGGER,
            name="Nobreak",
            update_interval=timedelta(seconds=2),
        )
        self.api = api

    async def _async_update_data(self):
        """Fetch data from API endpoint."""

        async with async_timeout.timeout(2):
            return await self.api.fetch_data()


class NobreakApi:
    """Communicates to the Nobreak API."""

    def __init__(self, endpoint: str, skip_ssl_validation: bool) -> None:
        """Initialize the nobreak API client."""
        self.endpoint = endpoint
        self.verify_ssl = not skip_ssl_validation

    async def exec_command(self, method: str, path: str):
        """Make API call."""
        async with aiohttp.ClientSession(
            connector=aiohttp.TCPConnector(verify_ssl=self.verify_ssl)
        ) as session:
            res = await session.request(method, self.endpoint + path)
            res.raise_for_status()
            return await res.json()

    async def fetch_data(self):
        """Fetch data."""
        return await self.exec_command("get", "/Nobreak")

    async def start_test(self):
        """Start a self test."""
        return await self.exec_command("post", "/Nobreak/Test/UntilFlat")

    async def finish_test(self):
        """Finish a test, if running."""
        return await self.exec_command("delete", "/Nobreak/Test")

    async def set_beep(self, beep_on: bool):
        """Set the beep state."""
        return await self.exec_command("post", "/Nobreak/Beep/" + str(beep_on))

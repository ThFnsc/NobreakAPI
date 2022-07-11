"""Nobreak switches."""

from __future__ import annotations

from homeassistant.components.switch import SwitchEntity
from homeassistant.config_entries import ConfigEntry
from homeassistant.core import HomeAssistant, callback
from homeassistant.helpers.entity_platform import AddEntitiesCallback
from homeassistant.helpers.update_coordinator import CoordinatorEntity

from . import NobreakApi, NobreakCoordinator
from .const import DATA_API, DATA_COORDINATOR, DOMAIN


async def async_setup_entry(
    hass: HomeAssistant,
    config_entry: ConfigEntry,
    async_add_entities: AddEntitiesCallback,
) -> None:
    """Set up switches."""
    coordinator = hass.data[DOMAIN][DATA_COORDINATOR]
    api = hass.data[DOMAIN][DATA_API]
    async_add_entities(
        [NobreakBeepSwitch(coordinator, api), NobreakTestSwitch(coordinator, api)]
    )


class NobreakBeepSwitch(CoordinatorEntity, SwitchEntity):
    """Nobreak beep switch."""

    _attr_name = "Beep"
    _attr_unique_id = "beep"
    _attr_icon = "mdi:volume-high"

    def __init__(self, coordinator: NobreakCoordinator, api: NobreakApi) -> None:
        """Initialize the nobreak beep switch."""
        super().__init__(coordinator)
        self.coordinator = coordinator
        self.api = api

    @callback
    def _handle_coordinator_update(self) -> None:
        self._attr_is_on = self.coordinator.data["beepOn"]
        self.async_write_ha_state()

    async def async_turn_on(self, **kwargs):
        """Turn the entity on."""
        await self.api.set_beep(True)

    async def async_turn_off(self, **kwargs):
        """Turn the entity off."""
        await self.api.set_beep(False)


class NobreakTestSwitch(CoordinatorEntity, SwitchEntity):
    """Nobreak test switch."""

    _attr_name = "Testing"
    _attr_unique_id = "test"
    _attr_icon = "mdi:test-tube"

    def __init__(self, coordinator: NobreakCoordinator, api: NobreakApi) -> None:
        """Initialize the nobreak test switch."""
        super().__init__(coordinator)
        self.coordinator = coordinator
        self.api = api

    @callback
    def _handle_coordinator_update(self) -> None:
        self._attr_is_on = self.coordinator.data["testExecuting"]
        self.async_write_ha_state()

    async def async_turn_on(self, **kwargs):
        """Turn the entity on."""
        await self.api.start_test()

    async def async_turn_off(self, **kwargs):
        """Turn the entity off."""
        await self.api.finish_test()

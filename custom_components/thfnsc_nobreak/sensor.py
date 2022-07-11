"""Nobreak sensors."""

from __future__ import annotations

from homeassistant.components.sensor import SensorDeviceClass, SensorEntity
from homeassistant.config_entries import ConfigEntry
from homeassistant.core import HomeAssistant, callback
from homeassistant.helpers.entity_platform import AddEntitiesCallback
from homeassistant.helpers.update_coordinator import (
    CoordinatorEntity,
    DataUpdateCoordinator,
)

from .const import DATA_COORDINATOR, DOMAIN


async def async_setup_entry(
    hass: HomeAssistant,
    config_entry: ConfigEntry,
    async_add_entities: AddEntitiesCallback,
) -> None:
    """Set up sensors."""
    coordinator = hass.data[DOMAIN][DATA_COORDINATOR]
    async_add_entities(
        [
            NobreakValue(
                coordinator, "voltageIn", "Voltage in", SensorDeviceClass.VOLTAGE, "V"
            ),
            NobreakValue(
                coordinator, "voltageOut", "Voltage out", SensorDeviceClass.VOLTAGE, "V"
            ),
            NobreakValue(
                coordinator,
                "batteryVoltage",
                "Battery voltage",
                SensorDeviceClass.VOLTAGE,
                "V",
                "mdi:battery-charging-100",
            ),
            NobreakValue(
                coordinator,
                "loadPercentage",
                "Output load",
                None,
                "%",
                "mdi:lightning-bolt",
            ),
            NobreakValue(
                coordinator,
                "frequencyHz",
                "Frequency",
                SensorDeviceClass.FREQUENCY,
                "Hz",
            ),
            NobreakValue(
                coordinator,
                "temperatureC",
                "Temperature",
                SensorDeviceClass.TEMPERATURE,
                "°C",
            ),
            NobreakValue(
                coordinator,
                "batteryHealthy",
                "Battery healthy",
                None,
                None,
                "mdi:battery-heart-variant",
            ),
            NobreakValue(
                coordinator,
                "powerSource",
                "Power source",
                None,
                None,
                "mdi:transmission-tower",
            ),
            NobreakValue(
                coordinator,
                "batteryPercentage",
                "Battery percentage",
                SensorDeviceClass.BATTERY,
                "%",
            ),
        ]
    )


class NobreakValue(CoordinatorEntity, SensorEntity):
    """Nobreak sensor."""

    def __init__(
        self,
        coordinator: DataUpdateCoordinator,
        propName: str,
        name: str,
        deviceClass: str | None,
        unit: str | None,
        icon: str | None = None,
    ) -> None:
        """Initialize a nobreak value."""
        super().__init__(coordinator)
        self.prop_name = propName
        self.coordinator = coordinator
        self._attr_name = name
        self._attr_unique_id = DOMAIN + "_" + propName
        self._attr_native_unit_of_measurement = unit
        self._attr_device_class = deviceClass
        self._attr_icon = icon

    @callback
    def _handle_coordinator_update(self) -> None:
        self._attr_native_value = self.coordinator.data[self.prop_name]
        self.async_write_ha_state()

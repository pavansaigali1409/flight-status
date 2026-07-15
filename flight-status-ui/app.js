const API_BASE = "http://localhost:5000";

const STATUS_LABELS = {
  0: "On Time",
  1: "Delayed",
  2: "Cancelled",
  3: "Diverted",
  4: "Unknown",
  OnTime: "On Time",
  Delayed: "Delayed",
  Cancelled: "Cancelled",
  Diverted: "Diverted",
  Unknown: "Unknown",
};

const STATUS_CLASSES = {
  0: "on-time",
  1: "delayed",
  2: "cancelled",
  3: "diverted",
  4: "unknown",
  OnTime: "on-time",
  Delayed: "delayed",
  Cancelled: "cancelled",
  Diverted: "diverted",
  Unknown: "unknown",
};

const form = document.getElementById("search-form");
const errorBlock = document.getElementById("error-block");
const resultCard = document.getElementById("result-card");
const statusBadge = document.getElementById("status-badge");
const resultTitle = document.getElementById("result-title");
const submitButton = form.querySelector('button[type="submit"]');

form.addEventListener("submit", async (event) => {
  event.preventDefault();
  clearError();
  hideResult();

  const flightNumber = form.flightNumber.value.trim();
  const date = form.date.value;

  submitButton.disabled = true;

  try {
    const url = new URL("/flights/status", API_BASE);
    url.searchParams.set("flightNumber", flightNumber);
    url.searchParams.set("date", date);

    const response = await fetch(url);

    if (!response.ok) {
      const body = await tryReadJson(response);
      const message = body?.error
        ?? `Request failed with status ${response.status}.`;
      showError(message);
      return;
    }

    const data = await response.json();
    renderResult(flightNumber, date, data);
  } catch (error) {
    showError(
      `Could not reach the API at ${API_BASE}. Start the API with dotnet run and ensure CORS is enabled. (${error.message})`
    );
  } finally {
    submitButton.disabled = false;
  }
});

function renderResult(flightNumber, date, data) {
  const statusKey = data.status;
  const label = STATUS_LABELS[statusKey] ?? "Unknown";
  const cssClass = STATUS_CLASSES[statusKey] ?? "unknown";

  resultTitle.textContent = `${flightNumber.toUpperCase()} · ${date}`;
  statusBadge.textContent = label;
  statusBadge.className = `status-badge ${cssClass}`;

  setText("scheduled-departure", formatDateTime(data.scheduledDepartureUtc));
  setText("scheduled-arrival", formatDateTime(data.scheduledArrivalUtc));
  setText("actual-departure", formatDateTime(data.actualDepartureUtc));
  setText("actual-arrival", formatDateTime(data.actualArrivalUtc));
  setText("source-provider", data.sourceProvider);
  setText("last-updated", formatDateTime(data.lastUpdatedUtc));

  setOptionalField("row-departure-terminal", "departure-terminal", data.departureTerminal);
  setOptionalField("row-arrival-terminal", "arrival-terminal", data.arrivalTerminal);
  setOptionalField("row-departure-gate", "departure-gate", data.departureGate);
  setOptionalField("row-arrival-gate", "arrival-gate", data.arrivalGate);
  setOptionalField("row-delay-reason", "delay-reason", data.delayReason);
  setOptionalField("row-message", "message", data.message);

  resultCard.classList.remove("hidden");
}

function setText(elementId, value) {
  document.getElementById(elementId).textContent = value ?? "—";
}

function setOptionalField(rowId, valueId, value) {
  const row = document.getElementById(rowId);
  const cell = document.getElementById(valueId);

  if (value) {
    row.classList.remove("hidden");
    cell.textContent = value;
  } else {
    row.classList.add("hidden");
    cell.textContent = "";
  }
}

function formatDateTime(value) {
  if (!value) {
    return null;
  }

  return new Date(value).toLocaleString(undefined, {
    dateStyle: "medium",
    timeStyle: "short",
    timeZone: "UTC",
  }) + " UTC";
}

function showError(message) {
  errorBlock.textContent = message;
  errorBlock.classList.remove("hidden");
}

function clearError() {
  errorBlock.textContent = "";
  errorBlock.classList.add("hidden");
}

function hideResult() {
  resultCard.classList.add("hidden");
}

async function tryReadJson(response) {
  try {
    return await response.json();
  } catch {
    return null;
  }
}

// Calls the Hotel Reservations Web API with fetch() and shows the JSON as a table.

// Columns shown for each resource (the service models have a few more fields; those appear in single-record view).
const columns = {
    guests: ["id", "fullName", "email", "phone", "city", "country", "guestSince", "reservationCount"],
    rooms: ["id", "roomNumber", "floor", "roomType", "nightlyRate", "maxOccupancy", "isSmoking", "isOutOfService"],
    roomtypes: ["id", "name", "nightlyRate", "maxOccupancy", "isActive", "roomCount", "description"],
    reservations: ["id", "guestName", "roomNumber", "roomType", "checkIn", "checkOut", "nights", "totalCost", "status"],
};

let resource = "guests";

const $ = id => document.getElementById(id);

// index.html?api=http://host:port points the client at a different API.
const apiParam = new URLSearchParams(location.search).get("api");
if (apiParam) $("apiBase").value = apiParam;

function selectTab(name) {
    const tab = document.querySelector(`#tabs button[data-resource="${name}"]`);
    if (!tab) return false;
    document.querySelectorAll("#tabs button").forEach(b => b.classList.toggle("active", b === tab));
    resource = name;
    $("result").innerHTML = "";
    setStatus(`Table: ${tab.textContent}. Click Get all or Get by ID.`);
    return true;
}

document.querySelectorAll("#tabs button").forEach(tab =>
    tab.addEventListener("click", () => selectTab(tab.dataset.resource)));

$("getAll").addEventListener("click", () => load(`/api/${resource}`));
$("getById").addEventListener("click", () => load(`/api/${resource}/${encodeURIComponent($("recordId").value)}`));
$("recordId").addEventListener("keydown", e => { if (e.key === "Enter") $("getById").click(); });

// Bookmarkable requests: index.html#rooms runs "Get all" on Rooms, index.html#rooms/5 runs "Get by ID" 5.
function runHash() {
    const [name, id] = decodeURIComponent(location.hash.slice(1)).split("/");
    if (!selectTab(name)) return;
    if (id) {
        $("recordId").value = id;
        $("getById").click();
    } else {
        $("getAll").click();
    }
}
window.addEventListener("hashchange", runHash);
runHash();

async function load(path) {
    const url = $("apiBase").value.replace(/\/+$/, "") + path;
    setStatus(`GET ${url} ...`);
    $("result").innerHTML = "";
    let response;
    try {
        response = await fetch(url, { headers: { Accept: "application/json" } });
    } catch (err) {
        // fetch() rejects with a bare TypeError for network and CORS failures; the browser hides the reason.
        setStatus(`GET ${url} failed: ${err.message}. Is the API running, and does its CORS policy allow ${location.origin}?`, "error");
        return;
    }

    const body = response.status === 204 ? null : await response.json();
    if (!response.ok) {
        setStatus(`GET ${url} → ${response.status} ${response.statusText}: ${body?.detail ?? body?.title ?? ""}`, "error");
        return;
    }

    if (Array.isArray(body)) {
        setStatus(`GET ${url} → ${response.status} OK, ${body.length} record(s)`, "ok");
        $("result").append(listTable(body, columns[resource]));
    } else {
        setStatus(`GET ${url} → ${response.status} OK`, "ok");
        $("result").append(recordTable(body));
    }
}

function listTable(rows, cols) {
    const table = document.createElement("table");
    table.innerHTML = `<thead><tr>${cols.map(c => `<th>${label(c)}</th>`).join("")}</tr></thead>`;
    const tbody = table.createTBody();
    for (const row of rows) {
        const tr = tbody.insertRow();
        for (const c of cols)
            tr.insertCell().textContent = format(c, row[c]);
    }
    return table;
}

function recordTable(record) {
    const table = document.createElement("table");
    table.className = "record";
    const tbody = table.createTBody();
    for (const [key, value] of Object.entries(record)) {
        const tr = tbody.insertRow();
        tr.insertCell().textContent = label(key);
        tr.insertCell().textContent = format(key, value);
    }
    return table;
}

const money = new Intl.NumberFormat("en-US", { style: "currency", currency: "USD" });

function format(key, value) {
    if (value === null || value === undefined) return "";
    if (typeof value === "boolean") return value ? "Yes" : "No";
    if (/rate|cost/i.test(key)) return money.format(value);
    return String(value);
}

// "reservationCount" -> "Reservation count"
function label(key) {
    const words = key.replace(/([A-Z])/g, " $1").toLowerCase();
    return words.charAt(0).toUpperCase() + words.slice(1);
}

function setStatus(text, kind = "") {
    $("status").textContent = text;
    $("status").className = "status " + kind;
}

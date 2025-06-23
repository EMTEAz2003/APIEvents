const apiUrl = "http://localhost:15191"; // <<< שנה לפורט שלך!

function resetForm() {
    document.getElementById("createEventForm").reset();
    document.getElementById("eventId").value = "";
    document.getElementById("eventSubmitBtn").innerText = "הוסף אירוע";
    document.getElementById("cancelEditBtn").style.display = "none";
}

// הוספת/עדכון אירוע
document.getElementById("createEventForm").addEventListener("submit", async function (e) {
    e.preventDefault();
    const id = document.getElementById("eventId").value;
    const data = {
        id: id ? +id : 0,
        title: document.getElementById("eventTitle").value,
        startDate: document.getElementById("eventStart").value,
        endDate: document.getElementById("eventEnd").value,
        maxRegistrations: +document.getElementById("eventMax").value,
        location: document.getElementById("eventLocation").value
    };
    const msgDiv = document.getElementById("createEventMsg");

    try {
        let res;
        if (id) {
            // עדכון
            res = await fetch(`${apiUrl}/event/${id}`, {
                method: "PUT",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(data)
            });
        } else {
            // יצירה
            res = await fetch(`${apiUrl}/event`, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(data)
            });
        }
        if (res.ok) {
            msgDiv.innerText = id ? "האירוע עודכן בהצלחה!" : "האירוע נוסף בהצלחה!";
            msgDiv.style.color = "green";
            setTimeout(() => { msgDiv.innerText = ""; }, 3000);
            resetForm();
            loadEvents();
        } else {
            msgDiv.innerText = "אירעה שגיאה. בדוק את הנתונים ונסה שוב.";
            msgDiv.style.color = "red";
        }
    } catch (err) {
        msgDiv.innerText = "שגיאה כללית: " + err;
        msgDiv.style.color = "red";
    }
});

// ביטול עריכה
document.getElementById("cancelEditBtn").onclick = resetForm;

// טען את כל האירועים
async function loadEvents() {
    const div = document.getElementById("eventsList");
    div.innerHTML = "טוען...";
    try {
        const res = await fetch(`${apiUrl}/schedule`);
        if (!res.ok) {
            div.innerHTML = "שגיאה בטעינת האירועים.";
            return;
        }
        const events = await res.json();
        if (events.length === 0) {
            div.innerHTML = "אין אירועים להצגה.";
            return;
        }
        let html = `<table><tr>
            <th>מספר</th><th>שם</th><th>התחלה</th><th>סיום</th><th>מיקום</th><th>משתתפים</th><th>פעולות</th>
            </tr>`;
        for (let ev of events) {
            html += `<tr>
                <td>${ev.id}</td>
                <td>${ev.title}</td>
                <td>${ev.startDate ? ev.startDate.replace("T", " ").slice(0, 16) : ""}</td>
                <td>${ev.endDate ? ev.endDate.replace("T", " ").slice(0, 16) : ""}</td>
                <td>${ev.location}</td>
                <td>${ev.maxRegistrations}</td>
                <td>
                    <button onclick="editEvent(${ev.id})">ערוך</button>
                    <button onclick="deleteEvent(${ev.id})">מחק</button>
                </td>
            </tr>`;
        }
        html += "</table>";
        div.innerHTML = html;
    } catch {
        div.innerHTML = "שגיאה כללית בטעינת האירועים.";
    }
}

// עריכת אירוע
window.editEvent = async function (id) {
    try {
        const res = await fetch(`${apiUrl}/event/${id}`);
        if (!res.ok) {
            alert("שגיאה בשליפת האירוע לעריכה.");
            return;
        }
        const ev = await res.json();
        document.getElementById("eventId").value = ev.id;
        document.getElementById("eventTitle").value = ev.title;
        document.getElementById("eventStart").value = ev.startDate.slice(0, 16);
        document.getElementById("eventEnd").value = ev.endDate.slice(0, 16);
        document.getElementById("eventMax").value = ev.maxRegistrations;
        document.getElementById("eventLocation").value = ev.location;
        document.getElementById("eventSubmitBtn").innerText = "עדכן אירוע";
        document.getElementById("cancelEditBtn").style.display = "inline-block";
    } catch {
        alert("שגיאה כללית בשליפת אירוע.");
    }
}

// מחיקת אירוע
window.deleteEvent = async function (id) {
    if (!confirm("למחוק את האירוע?")) return;
    try {
        const res = await fetch(`${apiUrl}/event/${id}`, { method: "DELETE" });
        if (res.ok) {
            alert("האירוע נמחק.");
            loadEvents();
        } else {
            alert("מחיקה נכשלה.");
        }
    } catch {
        alert("שגיאה כללית במחיקה.");
    }
}

// טען אוטומטית אירועים
window.onload = loadEvents;

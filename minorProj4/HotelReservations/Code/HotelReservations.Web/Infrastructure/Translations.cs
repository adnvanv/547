namespace HotelReservations.Web.Infrastructure;

/// <summary>
/// Interface text for the Language setting. English text is the key; a missing
/// translation falls back to the English text so a page never shows a blank label.
/// </summary>
public static class Translations
{
    private static readonly Dictionary<string, Dictionary<string, string>> Languages = new()
    {
        ["es"] = new()
        {
            // Navigation and common buttons
            ["Home"] = "Inicio",
            ["Guests"] = "Huéspedes",
            ["Rooms"] = "Habitaciones",
            ["Reservations"] = "Reservas",
            ["Administration"] = "Administración",
            ["Settings"] = "Configuración",
            ["Admin mode"] = "Modo admin",
            ["Edit"] = "Editar",
            ["Delete"] = "Eliminar",
            ["Cancel"] = "Cancelar",
            ["Save changes"] = "Guardar cambios",
            ["Search"] = "Buscar",
            ["Clear"] = "Limpiar",
            ["-- choose --"] = "-- elegir --",

            // Dashboard
            ["Welcome to Lakeside Hotel"] = "Bienvenido a Lakeside Hotel",
            ["Manage guests, rooms and reservations."] = "Administre huéspedes, habitaciones y reservas.",
            ["Rooms in service"] = "Habitaciones en servicio",
            ["Occupied tonight"] = "Ocupadas esta noche",
            ["Active room types"] = "Tipos de habitación activos",
            ["Upcoming arrivals"] = "Próximas llegadas",
            ["No upcoming arrivals."] = "No hay llegadas próximas.",

            // Guests
            ["Add guest"] = "Agregar huésped",
            ["Edit guest"] = "Editar huésped",
            ["Delete guest"] = "Eliminar huésped",
            ["Guest"] = "Huésped",
            ["Guest ID"] = "ID de huésped",
            ["Guest since"] = "Huésped desde",
            ["First name"] = "Nombre",
            ["Last name"] = "Apellido",
            ["Name"] = "Nombre",
            ["Email"] = "Correo electrónico",
            ["Phone"] = "Teléfono",
            ["City"] = "Ciudad",
            ["Country"] = "País",
            ["Stays"] = "Estancias",
            ["Search in"] = "Buscar en",
            ["Match"] = "Coincidencia",
            ["Search text"] = "Texto de búsqueda",
            ["Any field (or guest ID)"] = "Cualquier campo (o ID)",
            ["contains"] = "contiene",
            ["starts with"] = "empieza con",
            ["ends with"] = "termina con",
            ["guest(s) found for"] = "huésped(es) encontrados para",
            ["Showing all"] = "Mostrando los",
            ["guests"] = "huéspedes",
            ["No reservations yet."] = "Todavía no hay reservas.",
            ["Are you sure you want to delete this guest?"] = "¿Seguro que desea eliminar este huésped?",
            ["This guest has reservations, so the database will block the delete until they are removed."] =
                "Este huésped tiene reservas; la base de datos bloqueará la eliminación hasta que se eliminen.",

            // Rooms
            ["Room"] = "Habitación",
            ["Add room"] = "Agregar habitación",
            ["Edit room"] = "Editar habitación",
            ["Delete room"] = "Eliminar habitación",
            ["Room number"] = "Número de habitación",
            ["Floor"] = "Piso",
            ["Room type"] = "Tipo de habitación",
            ["Nightly rate"] = "Tarifa por noche",
            ["Sleeps"] = "Capacidad",
            ["Notes"] = "Notas",
            ["Available"] = "Disponible",
            ["Out of service"] = "Fuera de servicio",
            ["Smoking"] = "Fumadores",
            ["Smoking room"] = "Habitación para fumadores",
            ["Room types and prices are managed on the Administration page."] =
                "Los tipos de habitación y los precios se administran en la página de Administración.",
            ["Are you sure you want to delete this room?"] = "¿Seguro que desea eliminar esta habitación?",
            ["This room has reservations, so the database will block the delete. Consider marking it out of service instead."] =
                "Esta habitación tiene reservas; la base de datos bloqueará la eliminación. Márquela fuera de servicio.",

            // Reservations
            ["Reservation"] = "Reserva",
            ["New reservation"] = "Nueva reserva",
            ["Edit reservation"] = "Editar reserva",
            ["Delete reservation"] = "Eliminar reserva",
            ["Book reservation"] = "Reservar",
            ["reservations"] = "reservas",
            ["Check-in"] = "Entrada",
            ["Check-out"] = "Salida",
            ["Dates"] = "Fechas",
            ["Nights"] = "Noches",
            ["night(s)"] = "noche(s)",
            ["Status"] = "Estado",
            ["Total"] = "Total",
            ["Booked"] = "Reservada",
            ["Checked In"] = "Registrada",
            ["Checked Out"] = "Finalizada",
            ["Cancelled"] = "Cancelada",
            ["Cancelled reservations are hidden (see Settings)."] = "Las reservas canceladas están ocultas (ver Configuración).",
            ["Are you sure you want to delete this reservation?"] = "¿Seguro que desea eliminar esta reserva?",
            ["Tip: to keep a record of the booking, edit it and set the status to Cancelled instead."] =
                "Consejo: para conservar el registro, edítela y cambie el estado a Cancelada.",

            // Administration
            ["Administration: room types"] = "Administración: tipos de habitación",
            ["Add room type"] = "Agregar tipo",
            ["Edit room type"] = "Editar tipo de habitación",
            ["Delete room type"] = "Eliminar tipo de habitación",
            ["Max occupancy"] = "Ocupación máxima",
            ["Description"] = "Descripción",
            ["Active"] = "Activo",
            ["Inactive"] = "Inactivo",
            ["Active (can be assigned to rooms)"] = "Activo (se puede asignar a habitaciones)",
            ["Room types and nightly rates can only be changed here, in administrator mode. Inactive types are hidden from the room form but kept for rooms that already use them."] =
                "Los tipos de habitación y las tarifas solo se cambian aquí, en modo administrador. Los tipos inactivos se ocultan del formulario pero se conservan para las habitaciones que ya los usan.",
            ["Are you sure you want to delete this room type?"] = "¿Seguro que desea eliminar este tipo de habitación?",
            ["Rooms still use this type, so the database will block the delete. Mark it inactive instead."] =
                "Hay habitaciones con este tipo; la base de datos bloqueará la eliminación. Márquelo inactivo.",
            ["Administration is locked"] = "La administración está bloqueada",
            ["Room types and nightly rates can only be changed by an administrator. Turn on Administrator mode on the Settings page to continue."] =
                "Solo un administrador puede cambiar los tipos de habitación y las tarifas. Active el modo administrador en Configuración para continuar.",
            ["Go to Settings"] = "Ir a Configuración",

            // Settings
            ["These preferences are saved in this browser."] = "Estas preferencias se guardan en este navegador.",
            ["Appearance"] = "Apariencia",
            ["Theme"] = "Tema",
            ["Light"] = "Claro",
            ["Dark"] = "Oscuro",
            ["Match my system"] = "Igual que mi sistema",
            ["Accent color"] = "Color de acento",
            ["Primary"] = "Azul",
            ["Success"] = "Verde",
            ["Danger"] = "Rojo",
            ["Warning"] = "Amarillo",
            ["Info"] = "Celeste",
            ["Language and dates"] = "Idioma y fechas",
            ["Language"] = "Idioma",
            ["Date format"] = "Formato de fecha",
            ["Behavior"] = "Comportamiento",
            ["Show cancelled reservations"] = "Mostrar reservas canceladas",
            ["Ask for confirmation before deleting"] = "Pedir confirmación antes de eliminar",
            ["Administrator mode"] = "Modo administrador",
            ["unlocks the Administration page for editing room types"] = "desbloquea la página de Administración para editar tipos de habitación",
            ["Save settings"] = "Guardar configuración",
            ["Reset to defaults"] = "Restablecer valores",
            ["Settings saved."] = "Configuración guardada.",

            // Status messages
            ["Guest saved:"] = "Huésped guardado:",
            ["Guest deleted:"] = "Huésped eliminado:",
            ["Room saved:"] = "Habitación guardada:",
            ["Room deleted:"] = "Habitación eliminada:",
            ["Reservation saved."] = "Reserva guardada.",
            ["Reservation deleted:"] = "Reserva eliminada:",
            ["Room type saved:"] = "Tipo de habitación guardado:",
            ["Room type deleted:"] = "Tipo de habitación eliminado:",

            // Data-layer errors (fixed messages from HotelReservations.Data)
            ["That record no longer exists. It may have been deleted already."] =
                "Ese registro ya no existe. Es posible que ya se haya eliminado.",
            ["Another record already uses that value (emails, room numbers and room type names must be unique)."] =
                "Otro registro ya usa ese valor (los correos, números de habitación y nombres de tipo deben ser únicos).",
            ["This guest still has reservations. Delete or reassign those reservations first."] =
                "Este huésped todavía tiene reservas. Elimine o reasigne esas reservas primero.",
            ["This room has reservations. Delete those reservations first, or mark the room out of service."] =
                "Esta habitación tiene reservas. Elimine esas reservas primero o márquela fuera de servicio.",
            ["Rooms still use this room type. Change those rooms first, or mark this type inactive instead."] =
                "Todavía hay habitaciones con este tipo. Cámbielas primero o marque este tipo como inactivo.",
        },
    };

    public static string Get(string language, string english) =>
        Languages.TryGetValue(language, out var table) && table.TryGetValue(english, out var text) ? text : english;
}

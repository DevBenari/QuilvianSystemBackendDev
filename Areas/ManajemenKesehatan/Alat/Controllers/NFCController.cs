using Microsoft.AspNetCore.Mvc;
using PCSC;
using PCSC.Utils;
using System;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Alat.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NfcController : ControllerBase
    {
        [HttpGet("read")]
        public IActionResult ReadUID()
        {
            try
            {
                using var context = ContextFactory.Instance.Establish(SCardScope.System);
                var readers = context.GetReaders();

                if (readers.Length == 0)
                    return BadRequest(new { message = "❌ Tidak ada NFC reader terdeteksi." });

                var readerName = readers[0];
                using var reader = new SCardReader(context);

                var connect = reader.Connect(readerName, SCardShareMode.Shared, SCardProtocol.Any);
                if (connect != SCardError.Success)
                    return BadRequest(new { message = $"❌ Gagal koneksi ke reader: {SCardHelper.StringifyError(connect)}" });

                byte[] getUID = { 0xFF, 0xCA, 0x00, 0x00, 0x00 };
                byte[] response = new byte[256];
                int responseLength = response.Length;

                var sendPci = SCardPCI.GetPci(reader.ActiveProtocol);
                var transmit = reader.Transmit(sendPci, getUID, getUID.Length, new SCardPCI(), response, ref responseLength);

                if (transmit != SCardError.Success)
                    return BadRequest(new { message = $"❌ Gagal membaca UID: {SCardHelper.StringifyError(transmit)}" });

                var uid = BitConverter.ToString(response, 0, responseLength).Replace("-", "");
                return Ok(new { uid });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"❌ Error: {ex.Message}" });
            }
        }
    }
}
